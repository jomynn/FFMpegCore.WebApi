public class ApiRequest
{
    public required string Method { get; set; } // Name of the method to execute
    public required Dictionary<string, object> Parameters { get; set; } // Parameters for the method
}
