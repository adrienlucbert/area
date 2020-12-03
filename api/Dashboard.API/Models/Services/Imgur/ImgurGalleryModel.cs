using System.Linq;
using Imgur.API.Models;
using Newtonsoft.Json;

namespace Dashboard.API.Models.Services.Imgur
{
    public class ImgurGalleryItemModel
    {
        public ImgurGalleryItemModel()
        { }

        public ImgurGalleryItemModel(IGalleryAlbum galleryAlbum)
        {
            var coverImage = galleryAlbum.Images.FirstOrDefault();

            Cover = coverImage?.Link;
            Title = galleryAlbum.Title;
            Description = galleryAlbum.Description;
            Link = galleryAlbum.Link;
        }

        public ImgurGalleryItemModel(IGalleryImage galleryImage)
        {
            Cover = galleryImage.Link;
            Title = galleryImage.Title;
            Description = galleryImage.Description;
            Link = galleryImage.Link;
        }

        [JsonProperty("title")]
        public string? Title { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("cover")]
        public string? Cover { get; set; }

        [JsonProperty("link")]
        public string? Link { get; set; }
    }
}