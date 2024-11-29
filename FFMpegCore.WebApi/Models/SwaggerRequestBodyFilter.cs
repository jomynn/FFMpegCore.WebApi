using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class SwaggerRequestBodyFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Safely check if HttpMethod is POST
        if (context.ApiDescription.HttpMethod?.Equals("POST", System.StringComparison.OrdinalIgnoreCase) == true &&
            context.ApiDescription.ParameterDescriptions.Count > 0)
        {
            // Define the request body schema for file uploads
            operation.RequestBody = new OpenApiRequestBody
            {
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = new Dictionary<string, OpenApiSchema>
                            {
                                {
                                    "videoFile",
                                    new OpenApiSchema
                                    {
                                        Type = "string",
                                        Format = "binary"
                                    }
                                },
                                {
                                    "audioFile",
                                    new OpenApiSchema
                                    {
                                        Type = "string",
                                        Format = "binary"
                                    }
                                },
                                {
                                    "outputFileName",
                                    new OpenApiSchema
                                    {
                                        Type = "string"
                                    }
                                }
                            },
                            Required = new HashSet<string> { "videoFile", "audioFile", "outputFileName" }
                        }
                    }
                }
            };
        }
    }
}

