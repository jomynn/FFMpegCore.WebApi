using Newtonsoft.Json;

namespace FFMpegCore.WebApi.Models
{
    public class Scene
    {
        public required string Comment { get; set; }

        [JsonConverter(typeof(ElementConverter))]
        public required List<Element> Elements { get; set; }
    }
}
