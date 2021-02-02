using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Area.API.Models.Table.ManyToMany;
using Area.API.Models.Table.Owned;
using Newtonsoft.Json;

namespace Area.API.Models.Table
{
    [Table("Widgets")]
    public class WidgetModel
    {
        [ForeignKey("WidgetId")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = null!;

        [JsonProperty("description")]
        public string Description { get; set; } = null!;

        [JsonProperty("requires_auth")]
        public bool RequiresAuth { get; set; }

        [JsonIgnore]
        public int ServiceId { get; set; }

        [JsonProperty("service")]
        public ServiceModel Service { get; set; } = null!;

        [JsonIgnore]
        public ICollection<UserWidgetModel> Users { get; set; } = null!;

        [JsonProperty("params")]
        public ICollection<WidgetParamModel> Params { get; set; } = null!;
    }
}