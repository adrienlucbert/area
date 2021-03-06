using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Area.API.Constants;
using Area.API.Extensions;
using Area.API.Models.Table;
using Area.API.Models.Table.Owned;
using Area.API.Repositories;
using IpData;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Swan;
using Wangkanai.Detection.Services;

namespace Area.API.Services
{
    public class AuthService
    {
        public const string ClaimTypeUserId = "uid";
        private const string Algorithm = SecurityAlgorithms.HmacSha256;
        private readonly IConfiguration _configuration;
        private readonly TokenValidationParameters _validationParameters;
        private readonly IDetectionService _detection;
        private readonly IpDataClient _ipDataClient;
        private readonly UserRepository _userRepository;

        public AuthService(IConfiguration configuration, TokenValidationParameters validationParameters,
            IDetectionService detection, IpDataClient ipDataClient, UserRepository userRepository)
        {
            _configuration = configuration;
            _validationParameters = validationParameters;
            _detection = detection;
            _ipDataClient = ipDataClient;
            _userRepository = userRepository;
        }

        public class AuthenticationResult
        {
            public string? Code { get; set; }
            public string? Error { get; set; }
            public bool Successful => Error == null;
        }
        
        public async Task<AuthenticationResult> AuthenticateExternalUserAsync(string email, UserModel.UserType type)
        {
            string claimType = type.ToString();
            IdentityResult? identityResult = null;
            string? code = null;
            var user = _userRepository.GetUser(email: email, asNoTracking: false);

            if (user == null) {
                user = new UserModel {
                    UserName = email,
                    Email = email,
                    Type = type
                };
                identityResult = await _userRepository.AddUserAsync(user);
            } else if (user.Type != type) {
                return new AuthenticationResult {
                    Error = "Username or email already in use"
                };
            }

            if (identityResult == null || identityResult.Succeeded) {
                var claims = await _userRepository.GetUserClaimsAsync(user);
                identityResult = await _userRepository.RemoveUserClaimsAsync(user, claims.Where(claim =>
                    claim.Type == claimType));

                if (identityResult.Succeeded) {
                    code = GenerateToken(user.Id, DateTime.UtcNow.AddTicks(AuthConstants.CodeLifespanTicks), new List<Claim>(), claimType);
                    identityResult = await _userRepository.AddUserClaimAsync(user, new Claim(claimType, code));
                }
            }

            return new AuthenticationResult {
                Code = identityResult?.Succeeded == true ? code : null,
                Error = identityResult?.Succeeded == false ? identityResult.Errors.FirstOrDefault()?.Description ?? "Authentication failed" : null
            };
        }

        public async Task<string> GenerateAccessToken(int userId, IPAddress ipAddress)
        {
            var claims = new List<Claim>(new[] {
                new Claim(JwtRegisteredClaimNames.Typ, "access_token")
            });

            return await GenerateTokenWithDevice(userId, DateTime.UtcNow.AddTicks(AuthConstants.RefreshTokenLifespanTicks), ipAddress, claims);
        }

        public async Task<string> GenerateRefreshToken(int userId, IPAddress ipAddress)
        {
            var claims = new List<Claim>(new[] {
                new Claim(JwtRegisteredClaimNames.Typ, "refresh_token")
            });

            return await GenerateTokenWithDevice(userId, DateTime.UtcNow.AddTicks(AuthConstants.RefreshTokenLifespanTicks), ipAddress, claims);
        }

        private string GenerateToken(int userId, DateTime expiryTime, ICollection<Claim> claims, string issuer)
        {
            claims.Add(new Claim(ClaimTypeUserId, userId.ToString()));

            var signingCredentials = new SigningCredentials(_validationParameters.IssuerSigningKey, Algorithm);
            var token = new JwtSecurityToken(
                issuer,
                _configuration[AuthConstants.ValidAudience],
                claims,
                DateTime.UtcNow,
                expiryTime,
                signingCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<string> GenerateTokenWithDevice(int userId, DateTime expiryTime, IPAddress ipAddress, List<Claim> claims)
        {
            var country = await _ipDataClient.GetCountry(ipAddress);
            var device = new UserDeviceModel(_detection, userId);

            if (country != null)
                device.Country = country;

            device = AssociateDeviceToUser(userId, device);

            claims.Add(new Claim(JwtRegisteredClaimNames.AuthTime, device.FirstUsed.ToString()));

            return GenerateToken(userId, expiryTime, claims, device.Id.ToString());
        }

        private UserDeviceModel AssociateDeviceToUser(int userId, UserDeviceModel device)
        {
            var user = _userRepository.GetUser(userId, asNoTracking: false);

            var existingDevice = user!.Devices
                                    .FirstOrDefault(model => model.UserId == device.UserId
                                    && model.Architecture == device.Architecture
                                    && model.Browser == device.Browser
                                    && model.Country == device.Country
                                    && model.Os == device.Os
                                    && model.BrowserVersion == device.BrowserVersion
                                    && model.OsVersion == device.OsVersion);

            if (existingDevice != null) {
                existingDevice.LastUsed = device.LastUsed;
                return existingDevice;
            }

            user.Devices.Add(device);
            return device;
        }

        public async Task<bool> ValidateDeviceUse(ClaimsPrincipal principal, UserModel user, IPAddress? ipv4 = null)
        {
            if (!principal.TryGetAuthTime(out var authTime)
                || !principal.TryGetDeviceId(out var registeredDeviceId))
                return false;

            var registeredDevice = user.Devices.FirstOrDefault(model => model.Id == registeredDeviceId);
            if (registeredDevice == null)
                return false;

            var currentDevice = new UserDeviceModel(_detection, user.Id, authTime);
            if (registeredDeviceId != currentDevice.Id)
                return false;
            if (ipv4 != null) {
                var currentCountry = await _ipDataClient.GetCountry(ipv4) ?? registeredDevice.Country;
                if (registeredDevice.Country != currentCountry)
                    return false;
            }

            registeredDevice.LastUsed = DateTime.UtcNow.ToUnixEpochDate();

            return true;
        }

        private async Task<bool> ValidateDeviceUse(ClaimsPrincipal principal, IPAddress ipv4)
        {
            var user = principal.TryGetUserId(out var userId)
                ? _userRepository.GetUser(userId, asNoTracking: false)
                : null;
            if (user == null)
                return false;

            return await ValidateDeviceUse(principal, user, ipv4);
        }

        public async Task<ClaimsPrincipal?> ValidateRefreshToken(string refreshToken, IPAddress ipv4)
        {
            if (!TryGetPrincipalFromToken(refreshToken, out var claimsPrincipal))
                return null;

            try {
                var typ = claimsPrincipal.Claims.First(claim => claim.Type == JwtRegisteredClaimNames.Typ);
                if (typ.Value != "refresh_token")
                    return null;

                if (await ValidateDeviceUse(claimsPrincipal, ipv4))
                    return claimsPrincipal;
                return null;
            } catch {
                return null;
            }
        }

        public bool TryGetPrincipalFromToken(string token, out ClaimsPrincipal principals)
        {
            try {
                principals =
                    new JwtSecurityTokenHandler().ValidateToken(token, _validationParameters, out var validatedToken);
                return IsJwtWithValidSecurityAlgorithm(validatedToken);
            } catch {
                principals = new ClaimsPrincipal();
                return false;
            }
        }

        private static bool IsJwtWithValidSecurityAlgorithm(SecurityToken validatedToken)
        {
            return validatedToken is JwtSecurityToken jwtSecurityToken
                && jwtSecurityToken.Header.Alg.Equals(Algorithm, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}