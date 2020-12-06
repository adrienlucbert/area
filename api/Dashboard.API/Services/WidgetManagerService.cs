using System.Collections.Generic;
using System.Linq;
using Dashboard.API.Exceptions.Http;
using Dashboard.API.Models.Table.Owned;
using Dashboard.API.Repositories;
using Dashboard.API.Services.Widgets;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;

namespace Dashboard.API.Services
{
    public class WidgetCallParameters
    {
        public IDictionary<string, int> Integers { get; set; } = new Dictionary<string, int>();
        public IDictionary<string, string> Strings { get; set; } = new Dictionary<string, string>();
    }

    public class WidgetManagerService
    {
        private readonly DatabaseRepository _database;
        private readonly IDictionary<string, IWidgetService> _widgets;

        public WidgetManagerService(
            DatabaseRepository database,
            ImgurGalleryWidgetService imgurGallery,
            ImgurFavoritesWidgetService imgurFavorites)
        {
            _database = database;
            _widgets = new Dictionary<string, IWidgetService> {
                {imgurGallery.Name, imgurGallery},
                {imgurFavorites.Name, imgurFavorites}
            };
        }

        public JsonResult CallWidgetById(HttpContext context, int widgetId)
        {
            var userId = AuthService.GetUserIdFromPrincipal(context.User);
            if (userId == null)
                throw new UnauthorizedHttpException();

            var user = _database.Users
                .Include(model => model.WidgetParams!
                    .Where(paramModel => paramModel.WidgetId == widgetId))
                .FirstOrDefault(model => model.Id == userId);
            if (user == null)
                throw new UnauthorizedHttpException();

            var widget = _database.Widgets
                .Include(model => model.DefaultParams)
                .FirstOrDefault(model => model.Id == widgetId);
            if (widget == null || !_widgets.TryGetValue(widget.Name!, out var widgetService))
                throw new NotFoundHttpException("Widget not found");

            if (widget.RequiresAuth == true)
                ValidateSignInState(widgetService, widget.ServiceId!.Value);

            var widgetCallParams = BuildWidgetCallParams(
                widgetId,
                widget.DefaultParams ?? new List<WidgetParamModel>(),
                user.WidgetParams ?? new List<UserWidgetParamModel>(),
                context.Request.Query);

            return widgetService.CallWidgetApi(context, user, widget, widgetCallParams);
        }

        private WidgetCallParameters BuildWidgetCallParams(int widgetId, ICollection<WidgetParamModel> defaultParams, ICollection<UserWidgetParamModel> userParams, IQueryCollection queryParams)
        {
            UpdateUserParamsWithQueryParams(widgetId, defaultParams, userParams, queryParams);

            var callParams = new WidgetCallParameters();

            foreach (var defaultParam in defaultParams) {
                if (defaultParam.Type == "string") {
                    if (!callParams.Strings.TryAdd(defaultParam.Name!, defaultParam.Value!)) {
                        callParams.Strings[defaultParam.Name!] = defaultParam.Value!;
                    }
                } else {
                    int.TryParse(defaultParam.Value, out var integerValue);
                    if (!callParams.Integers.TryAdd(defaultParam.Name!, integerValue)) {
                        callParams.Integers[defaultParam.Name!] = integerValue;
                    }
                }
            }

            foreach (var userParam in userParams) {
                if (userParam.Type == "string") {
                    if (!callParams.Strings.TryAdd(userParam.Name!, userParam.Value!)) {
                        callParams.Strings[userParam.Name!] = userParam.Value!;
                    }
                } else {
                    int.TryParse(userParam.Value, out var integerValue);
                    if (!callParams.Integers.TryAdd(userParam.Name!, integerValue)) {
                        callParams.Integers[userParam.Name!] = integerValue;
                    }
                }
            }

            return callParams;
        }

        private void UpdateUserParamsWithQueryParams(int widgetId, ICollection<WidgetParamModel> defaultParams, ICollection<UserWidgetParamModel> userParams, IQueryCollection queryParams)
        {
            foreach (var (key, value) in queryParams) {
                var userParam = userParams.FirstOrDefault(model => model.Name == key);
                if (userParam != null) {
                    userParam.Value = GetParamValueByType(value, userParam.Type!);
                } else {
                    var defaultParam = defaultParams.FirstOrDefault(model => model.Name == key);
                    if (defaultParam == null)
                        continue;
                    userParams.Add(new UserWidgetParamModel {
                        Name = defaultParam.Name,
                        Type = defaultParam.Type,
                        WidgetId = widgetId,
                        Value = GetParamValueByType(value, defaultParam.Type!)
                    });
                }
            }

            _database.SaveChanges();
        }

        private static string GetParamValueByType(StringValues value, string paramType)
        {
            if (paramType == "string")
                return value;
            int.TryParse(value, out var integerValue);
            return integerValue.ToString();
        }

        private void ValidateSignInState(IWidgetService widgetService, int serviceId)
        {
            var serviceTokens = _database.Users
                .AsNoTracking()
                .Include(model => model.ServiceTokens!
                    .Where(tokensModel => tokensModel.ServiceId == serviceId))
                .SelectMany(model => model.ServiceTokens)
                .FirstOrDefault();
            if (serviceTokens == null)
                throw new UnauthorizedHttpException("You need to be signed-in to the service");
            if (widgetService.ValidateServiceAuth(serviceTokens))
                return;
            _database.Remove(serviceTokens);
            throw new UnauthorizedHttpException("You need to be sign-in again to the service");
        }
    }
}
