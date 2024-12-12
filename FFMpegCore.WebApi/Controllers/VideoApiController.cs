using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

[ApiController]
[Route("v2/api/video")]
public class VideoApiController : ControllerBase
{
    [HttpPost("accept")]
    public IActionResult ProcessVideoOperation([FromBody] JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Undefined || element.ValueKind == JsonValueKind.Null)
        {
            return BadRequest(new { error = "Invalid or empty JSON object." });
        }

        try
        {
            // Extract distinct "type" values
            var distinctTypes = GetDistinctTypes(element);

            return Ok(new
            {
                message = "Distinct 'type' values retrieved successfully.",
                types = distinctTypes
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    private static List<string> GetDistinctTypes(JsonElement element)
    {
        var types = new HashSet<string>(); // Use HashSet to ensure distinct values

        ExtractTypesRecursive(element, types);

        return types.ToList();
    }

    private static void ExtractTypesRecursive(JsonElement element, HashSet<string> types)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (property.Name == "type" &&   property.Value.ValueKind == JsonValueKind.String)
                {

                    var typeValue = property.Value.GetString();
                    if (!string.IsNullOrEmpty(typeValue))
                    {
                        types.Add(typeValue); // Add "type" value to the set if not null or empty
                    }
                }
                else
                {
                    // Recursively process nested objects or arrays
                    ExtractTypesRecursive(property.Value, types);
                }
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                // Recursively process array elements
                ExtractTypesRecursive(item, types);
            }
        }
    }

    [HttpPost("accept2")]
    public IActionResult AcceptAnyJson([FromBody] JObject jsonObject)
    {
        if (jsonObject == null || !jsonObject.HasValues)
        {
            return BadRequest(new { error = "Invalid or empty JSON object." });
        }

        try
        {
            // Example: Log the received JSON
            Console.WriteLine("Received JSON:");
            Console.WriteLine(jsonObject.ToString());

            // Example: Dynamically access a property if it exists
            if (jsonObject.ContainsKey("type"))
            {
                var type = jsonObject["type"]?.ToString();
                Console.WriteLine($"Type: {type}");
            }

            // Example: Return the received JSON in the response
            return Ok(new
            {
                message = "JSON processed successfully.",
                received = jsonObject
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
