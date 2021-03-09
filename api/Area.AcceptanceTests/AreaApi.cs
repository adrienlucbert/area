using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Area.AcceptanceTests.Constants;
using Area.AcceptanceTests.Models;
using Area.AcceptanceTests.Models.Requests;
using Area.AcceptanceTests.Models.Responses;
using Area.AcceptanceTests.Utilities;

namespace Area.AcceptanceTests
{
    public class AreaApi
    {
        public readonly AreaHttpClient Client = new AreaHttpClient("http://localhost:8080");

        private TokensModel? _tokens;

        public TokensModel? Tokens {
            get => _tokens;
            set {
                _tokens = value;
                Client.SetBearer(_tokens?.AccessToken);
            }
        }

        public async Task<ResponseHolder<ResponseModel<AboutDotJsonModel>>> AboutDotJson() =>
            await Client.GetAsync<ResponseModel<AboutDotJsonModel>>(RouteConstants.AboutDotJson);

        public async Task<ResponseHolder<StatusModel>> Register(RegisterModel form) =>
            await Client.PostAsync(RouteConstants.Users.Register, form);

        public async Task<ResponseHolder<ResponseModel<TokensModel>>> SignIn(SignInModel form) =>
            await Client.PostAsync<ResponseModel<TokensModel>, SignInModel>(RouteConstants.Auth.SignIn, form);

        public async Task<ResponseHolder<StatusModel>> DeleteMyUser() =>
            await Client.DeleteAsync(RouteConstants.Users.DeleteMyUser);

        public async Task<ResponseHolder<ResponseModel<UserInformationModel>>> GetMyUser() =>
            await Client.GetAsync<ResponseModel<UserInformationModel>>(RouteConstants.Users.GetMyUser);

        public async Task<ResponseHolder<ResponseModel<TokensModel>>> RefreshAccessToken(RefreshTokenModel form) =>
            await Client.PostAsync<ResponseModel<TokensModel>, RefreshTokenModel>(RouteConstants.Auth.RefreshAccessToken, form);

        public async Task<ResponseHolder<ResponseModel<IEnumerable<ServiceModel>>>> GetServices() =>
            await Client.GetAsync<ResponseModel<IEnumerable<ServiceModel>>>(RouteConstants.Services.GetServices);

        public async Task<ResponseHolder<ResponseModel<IEnumerable<ServiceModel>>>> GetMyServices() =>
            await Client.GetAsync<ResponseModel<IEnumerable<ServiceModel>>>(RouteConstants.Services.GetMyServices);

        public async Task<ResponseHolder<ResponseModel<ServiceModel>>> GetServiceById(int id) =>
            await Client.GetAsync<ResponseModel<ServiceModel>>(RouteConstants.Services.GetServiceById(id));

        public async Task<HttpResponseMessage> SignInServiceById(int id, ExternalAuthModel form) =>
            await Client.RawPostAsync(RouteConstants.Services.SignInServiceById(id), form);

        public async Task<ResponseHolder<StatusModel>> SignOutServiceById(int id) =>
            await Client.DeleteAsync(RouteConstants.Services.SignOutServiceById(id));

        public async Task<ResponseHolder<ResponseModel<IEnumerable<WidgetModel>>>> GetWidgets() =>
            await Client.GetAsync<ResponseModel<IEnumerable<WidgetModel>>>(RouteConstants.Widgets.GetWidgets);

        public async Task<ResponseHolder<ResponseModel<IEnumerable<WidgetModel>>>> GetWidgets(int serviceId) =>
            await Client.GetAsync<ResponseModel<IEnumerable<WidgetModel>>>(RouteConstants.Widgets.GetWidgets + $"?serviceId={serviceId}");

        public async Task<ResponseHolder<ResponseModel<IEnumerable<WidgetModel>>>> GetMyWidgets() =>
            await Client.GetAsync<ResponseModel<IEnumerable<WidgetModel>>>(RouteConstants.Widgets.GetMyWidgets);

        public async Task<ResponseHolder<ResponseModel<IEnumerable<WidgetModel>>>> GetMyWidgets(int serviceId) =>
            await Client.GetAsync<ResponseModel<IEnumerable<WidgetModel>>>(RouteConstants.Widgets.GetMyWidgets + $"?serviceId={serviceId}");

        public async Task<ResponseHolder<StatusModel>> SubscribeWidgetById(int id) =>
            await Client.PostAsync(RouteConstants.Widgets.SubscribeWidgetById(id));

        public async Task<ResponseHolder<StatusModel>> UnsubscribeWidgetById(int id) =>
            await Client.DeleteAsync(RouteConstants.Widgets.UnsubscribeWidgetById(id));
        
        public async Task<ResponseHolder<ResponseModel<WidgetResponseModel>>> CallWidgetById(int id, string queryParameters) =>
            await Client.GetAsync<ResponseModel<WidgetResponseModel>>(RouteConstants.Widgets.CallWidgetById(id) + queryParameters);

        public async Task<ResponseHolder<ResponseModel<WidgetResponseModel>>> CallWidgetById(int id) =>
            await Client.GetAsync<ResponseModel<WidgetResponseModel>>(RouteConstants.Widgets.CallWidgetById(id));

        public async Task<ResponseHolder<ResponseModel<DevicesModel>>> GetMyDevices() =>
            await Client.GetAsync<ResponseModel<DevicesModel>>(RouteConstants.Users.GetMyDevices);

        public async Task<ResponseHolder<StatusModel>> DeleteMyDevice(uint deviceId) =>
            await Client.DeleteAsync<StatusModel>(RouteConstants.Users.DeleteMyDevice(deviceId));

        public async Task<HttpResponseMessage> SignInWithFacebook(ExternalAuthModel form) =>
            await Client.RawPostAsync(RouteConstants.Auth.SignInWithFacebook, form);

        public async Task<HttpResponseMessage> SignInWithGoogle(ExternalAuthModel form) =>
            await Client.RawPostAsync(RouteConstants.Auth.SignInWithGoogle, form);

        public async Task<HttpResponseMessage> SignInWithMicrosoft(ExternalAuthModel form) =>
            await Client.RawPostAsync(RouteConstants.Auth.SignInWithMicrosoft, form);
    }
}