using Microsoft.AspNetCore.Mvc;

namespace FFMpegCore.WebApi.Controllers
{
    [ApiController]
    [Route("v2/api")]
    public class ApiController : ControllerBase
    {
        private readonly ILogger<ApiController> _logger;

        public ApiController(ILogger<ApiController> logger)
        {
            _logger = logger;
        }

        //_logger.LogInformation($"Executing {methodName} with parameters {JsonConvert.SerializeObject(parameters)}");

        [HttpPost]
        public IActionResult HandleRequest([FromBody] ApiRequest request)
        {
            //_logger.LogInformation($"Executing {methodName} with parameters {JsonConvert.SerializeObject(parameters)}");
            _logger.LogInformation("ok");

            if (string.IsNullOrEmpty(request.Method))
            {
                return BadRequest(new { error = "Method is required" });
            }

            try
            {
                // Call the appropriate method dynamically
                var result = ExecuteMethod(request.Method, request.Parameters);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        private static object ExecuteMethod(string methodName, Dictionary<string, object> parameters)
        {
            // Map method names to actual implementations
            return methodName.ToLower() switch
            {
                "method1" => Method1(parameters),
                "method2" => Method2(parameters),
                _ => throw new ArgumentException($"Unknown method: {methodName}")
            };
        }

        private static object Method1(Dictionary<string, object> parameters)
        {
            // Example: Extract parameter and perform action
            var param1 = parameters.ContainsKey("param1") ? parameters["param1"]?.ToString() : "default";
            return $"Method1 executed with param1 = {param1}";
        }

        private static object Method2(Dictionary<string, object> parameters)
        {
            // Another example implementation
            var param1 = parameters.ContainsKey("param1") ? parameters["param1"]?.ToString() : "default";
            var param2 = parameters.ContainsKey("param2") ? parameters["param2"]?.ToString() : "default";
            return $"Method2 executed with param1 = {param1} and param2 = {param2}";
        }
    }
}
