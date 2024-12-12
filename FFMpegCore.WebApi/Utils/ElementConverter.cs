using FFMpegCore.WebApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class ElementConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(Element);
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);
        var type = jsonObject["type"]?.ToString();

        if (string.IsNullOrEmpty(type))
        {
            throw new ArgumentException("Element 'type' is missing or null.");
        }

        Element element = type switch
        {
            "video" => new VideoElement { Type = type },
            "image" => new ImageElement { Type = type },
            "audio" => new AudioElement { Type = type },
            "text" => new TextElement { Type = type },
            _ => throw new ArgumentException($"Unknown element type: {type}")
        };

        serializer.Populate(jsonObject.CreateReader(), element);
        return element;
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value);
    }
}
