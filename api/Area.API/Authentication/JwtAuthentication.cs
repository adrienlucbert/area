using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Area.API.Repositories;
using Area.API.Utilities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Area.API.Authentication
{
    public class JwtAuthentication : JwtBearerHandler
    {
        private readonly UserRepository _userRepository;

        public JwtAuthentication(IOptionsMonitor<JwtBearerOptions> options, ILoggerFactory logger, UrlEncoder encoder,
            ISystemClock clock, UserRepository userRepository)
            : base(options, logger, encoder, clock)
        {
            _userRepository = userRepository;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var authenticateResult = await base.HandleAuthenticateAsync();

            if (!authenticateResult.Succeeded)
                return authenticateResult;

            var userId = AuthUtilities.GetUserIdFromPrincipal(authenticateResult.Principal);

            if (userId == null || !_userRepository.UserExists(userId.Value))
                return AuthenticateResult.Fail("The associated user does not exist");

            return authenticateResult;
        }
    }
}