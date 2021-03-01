using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Web;
using Area.API.Constants;
using Area.API.Exceptions.Http;
using Area.API.Extensions;
using Area.API.Models;
using Area.API.Models.Table;
using Area.API.Repositories;
using Area.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;

namespace Area.API.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [SwaggerTag("Service-related endpoints")]
    public class ServicesController : ControllerBase
    {
        private readonly ServiceManagerService _serviceManager;
        private readonly ServiceRepository _serviceRepository;
        private readonly UserRepository _userRepository;

        public ServicesController(ServiceManagerService serviceManager, ServiceRepository serviceRepository,
            UserRepository userRepository)
        {
            _serviceManager = serviceManager;
            _serviceRepository = serviceRepository;
            _userRepository = userRepository;
        }

        [HttpGet(RouteConstants.Services.GetServices)]
        [SwaggerOperation(
            Summary = "List all services",
            Description = "## Get a list of all services available"
        )]
        public ResponseModel<List<ServiceModel>> GetServices()
        {
            return new ResponseModel<List<ServiceModel>> {
                Data = _serviceRepository.GetServices().ToList()
            };
        }

        [HttpGet(RouteConstants.Services.GetMyServices)]
        [SwaggerOperation(
            Summary = "List a user's services",
            Description = "## Get a list of all services where a user is subscribed to some of its widget(s)"
        )]
        public ResponseModel<List<ServiceModel>> GetMyService()
        {
            if (!User.TryGetUserId(out var userId))
                throw new InternalServerErrorHttpException();

            var services = _serviceRepository.GetServicesByUser(userId);

            return new ResponseModel<List<ServiceModel>> {
                Data = services
            };
        }

        [HttpGet(RouteConstants.Services.GetService)]
        [SwaggerOperation(
            Summary = "Get a service",
            Description = "## Get a information about a service in particular"
        )]
        [SwaggerResponse((int) HttpStatusCode.NotFound, "The service does not exist")]
        public ResponseModel<ServiceModel> GetService(
            [FromRoute] [Required] [Range(1, int.MaxValue)]
            int? serviceId
        )
        {
            var service = _serviceRepository.GetService(serviceId!.Value);

            if (service == null)
                throw new NotFoundHttpException("This service does not exist");

            return new ResponseModel<ServiceModel> {
                Data = service
            };
        }

        [HttpPost(RouteConstants.Services.SignInService)]
        [SwaggerOperation(
            Summary = "Sign-in a user to a service",
            Description =
                @"## Sign-in the user to a service.
## If the service doesn't have sign-in capabilities, an empty success response is returned (a.k.a without `data`).
## Otherwise an authentication URL is returned as `data` to redirect the user to"
        )]
        public RedirectResult SignInService(
            [FromRoute] [Required] [Range(1, int.MaxValue)] [SwaggerParameter("Service's ID")]
            int? serviceId,
            [FromBody]
            [SwaggerSchema("Required information to redirect the user back to the client once the operation in done")]
            ExternalAuthModel body
        )
        {
            var state = new ServiceAuthStateModel {
                State = body.State,
                RedirectUrl = body.RedirectUrl
            };

            string? urlOrError = null;
            var signedIn = false;

            if (User.TryGetUserId(out state.UserId)) {
                signedIn = _serviceManager.TrySignInServiceById(serviceId!.Value, state, out urlOrError);
                if (signedIn && urlOrError != null)
                    return new RedirectResult(urlOrError);
            }

            var redirectUrl = new UriBuilder(body.RedirectUrl);
            var queryParams = HttpUtility.ParseQueryString(redirectUrl.Query);

            queryParams["state"] = body.State;
            if (!signedIn) {
                queryParams["successful"] = "false";
                queryParams["error"] = urlOrError ?? "Unauthorized";
            } else {
                queryParams["successful"] = "true";
            }

            redirectUrl.Query = queryParams.ToString();

            return new RedirectResult(redirectUrl.ToString());
        }

        [HttpDelete(RouteConstants.Services.SignOutService)]
        [SwaggerOperation(
            Summary = "Sign-out a user from a service",
            Description =
                "## Sign-out the user from a service. If the service doesn't have sign-in capabilities, an empty success response is returned (a.k.a. without `data`)"
        )]
        public StatusModel SignOutService(
            [FromRoute] [Required] [Range(1, int.MaxValue)] [SwaggerParameter("Service's ID")]
            int? serviceId
        )
        {
            if (!User.TryGetUserId(out var userId))
                throw new InternalServerErrorHttpException();

            _userRepository.RemoveServiceCredentials(userId, serviceId!.Value);
            return StatusModel.Success();
        }

        [HttpGet(RouteConstants.Services.SignInServiceCallback)]
        [AllowAnonymous]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Produces("text/html")]
        public IActionResult SignInServiceCallback(
            [FromRoute] [Required] [Range(1, int.MaxValue)]
            int? serviceId,
            [FromQuery]
            string state
        )
        {
            ServiceAuthStateModel serviceAuthState;

            try {
                serviceAuthState = JsonConvert.DeserializeObject<ServiceAuthStateModel>(HttpUtility.UrlDecode(state));
            } catch {
                Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                return Content("<h1>An unexpected error has occured, please try again later</h1>", "text/html");
            }

            bool failed;
            var redirectUrl = new UriBuilder(serviceAuthState.RedirectUrl);
            var queryParams = HttpUtility.ParseQueryString(redirectUrl.Query);
            queryParams["state"] = serviceAuthState.State;

            try {
                failed = !_serviceManager.HandleServiceSignInCallbackById(HttpContext, serviceId!.Value, serviceAuthState);
            } catch {
                failed = true;
            }

            if (failed) {
                queryParams["successful"] = "false";
                queryParams["error"] = "Unable to sign-in, please try again later.";
            } else {
                queryParams["successful"] = "true";
            }

            redirectUrl.Query = queryParams.ToString();

            return new RedirectResult(redirectUrl.ToString());
        }
    }
}