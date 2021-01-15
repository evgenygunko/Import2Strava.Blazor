using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Api.Functions
{
    public static class VersionGet
    {
        [FunctionName("VersionGet")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "version")] HttpRequest req,
            ILogger log)
        {
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            return new OkObjectResult(version);
        }
    }
}
