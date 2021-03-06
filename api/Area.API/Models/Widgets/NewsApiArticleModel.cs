using NewsAPI.Models;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using Swan;

namespace Area.API.Models.Widgets
{
    [SwaggerSchema("## This is the response model for all the widgets of the NewsApi service")]
    public class NewsApiArticleModel : WidgetCallResponseItemModel
    {
        public NewsApiArticleModel()
        { }

        public NewsApiArticleModel(Article article)
        {
            Header = article.Title;
            Content = article.Content;
            Link = article.Url;
            Image = article.UrlToImage;

            Source = article.Source.Name;
            Author = article.Author;
            Description = article.Description;
            PublishedAt = article.PublishedAt?.ToUnixEpochDate();
        }

        [JsonProperty("source", Required = Required.Always)]
        [SwaggerSchema("The source of the article")]
        public string Source { get; set; } = null!;

        [JsonProperty("author", Required = Required.Always)]
        [SwaggerSchema("The author of the article")]
        public string Author { get; set; } = null!;

        [JsonProperty("description", Required = Required.Always)]
        [SwaggerSchema("The description of the content")]
        public string Description { get; set; } = null!;

        [JsonProperty("published_at", Required = Required.DisallowNull)]
        [SwaggerSchema("The publishing date, in UTC Linux EPOCH, of the article")]
        public long? PublishedAt { get; set; }
    }
}