using System.Collections.Generic;
using System.Linq;
using Dashboard.API.Exceptions.Http;
using Dashboard.API.Models.Response;
using Dashboard.API.Models.Services.Imgur;
using Dashboard.API.Models.Table;
using Dashboard.API.Services.Services;
using Imgur.API.Enums;
using Imgur.API.Models.Impl;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dashboard.API.Services.Widgets
{
    public class ImgurGalleryWidgetService : IWidgetService
    {
        private ImgurServiceService Imgur { get; }

        private readonly IDictionary<string, GallerySection> _gallerySections = new Dictionary<string, GallerySection> {
            {"hot", GallerySection.Hot},
            {"top", GallerySection.Top},
            {"user", GallerySection.User},
        };

        public ImgurGalleryWidgetService(ImgurServiceService imgur)
        {
            Imgur = imgur;
        }

        public string Name { get; } = "Imgur public gallery";

        public JsonResult CallWidgetApi(HttpContext context, UserModel user, WidgetModel widget, WidgetCallParameters widgetCallParams)
        {
            if (Imgur.Client == null)
                throw new InternalServerErrorHttpException();
            var galleryEndpoint = new Imgur.API.Endpoints.Impl.GalleryEndpoint(Imgur.Client);

            var sectionStr = widgetCallParams.Strings["section"].ToLower();

            if (!_gallerySections.TryGetValue(sectionStr, out var section))
                section = _gallerySections.First().Value;

            var task = galleryEndpoint.GetGalleryAsync(section);
            task.Wait();
            if (!task.IsCompletedSuccessfully)
                throw new InternalServerErrorHttpException("Couldn't not reach Imgur's API");

            return new ResponseModel<List<ImgurGalleryItemModel>> {
                Data = ImgurServiceService.CoverImageListFromGallery(task.Result)
            };
        }
    }
}
