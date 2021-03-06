namespace Area.AcceptanceTests.Constants
{
    public static class RouteConstants
    {
        private const string Api = "/api";

        public const string AboutDotJson = Api + "/about";
        public const string InvalidRoute = "/this/route/leads/to/nowhere";

        public static class Auth
        {
            private const string Root = Api + "/auth";

            public const string ResetPassword = Root + "/password";
            public const string ChangePassword = Root + "/password";
            public const string SignIn = Root + "/token";
            public const string RefreshAccessToken = Root + "/refresh";
            public const string SignInWithFacebook = Root + "/facebook";
            public const string SignInWithGoogle = Root + "/google";
            public const string SignInWithMicrosoft = Root + "/microsoft";
        }

        public static class Users
        {
            private const string Root = Api + "/users";

            public const string Register = Root;
            public const string GetMyUser = Root + "/me";
            public const string DeleteMyUser = Root + "/me";
            public const string GetMyDevices = Root + "/me/devices";
            public static string DeleteMyDevice(uint deviceId) => Root + "/me/devices/" + deviceId;
        }

        public static class Services
        {
            private const string Root = Api + "/services";

            public const string GetServices = Root;
            public const string GetMyServices = Root + "/me";
            public static string GetServiceById(int id) => Root + "/" + id;
            public static string SignInServiceById(int id) => Root + "/auth/" + id;
            public static string SignOutServiceById(int id) => Root + "/auth/" + id;
        }

        public static class Widgets
        {
            private const string Root = Api + "/widgets";

            public const string GetWidgets = Root;
            public static string GetWidgetsByService(int serviceId) => GetWidgets + $"?serviceId={serviceId}";
            public const string GetMyWidgets = Root + "/me";
            public static string GetMyWidgetsByService(int serviceId) => GetMyWidgets + $"?serviceId={serviceId}";
            public static string CallWidgetById(int id) => Root + "/" + id;
            public static string SubscribeWidgetById(int id) => Root + "/" + id;
            public static string UnsubscribeWidgetById(int id) => Root + "/" + id;
        }
    }
}