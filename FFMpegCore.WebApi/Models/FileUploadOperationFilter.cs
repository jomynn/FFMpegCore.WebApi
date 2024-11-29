using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Check for parameters of type DTO with IFormFile fields
        var formFileParameters = context.ApiDescription.ParameterDescriptions
            .Where(p => p.ModelMetadata?.ContainerType != null && p.ModelMetadata?.ModelType == typeof(IFormFile))
            .ToList();

        if (formFileParameters.Any())
        {
            operation.RequestBody = new OpenApiRequestBody
            {
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = context.ApiDescription.ParameterDescriptions
                                .SelectMany(p =>
                                    p.ModelMetadata?.ContainerType?.GetProperties() ?? Array.Empty<System.Reflection.PropertyInfo>())
                                .Where(prop => prop.PropertyType == typeof(IFormFile) || prop.PropertyType == typeof(IEnumerable<IFormFile>))
                                .ToDictionary(
                                    prop => prop.Name,
                                    prop => new OpenApiSchema
                                    {
                                        Type = "string",
                                        Format = "binary"
                                    })
                        }
                    }
                }
            };
        }
    }
}
