using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Api.Functions
{
    public static class UserInfoGet
    {
        private class ClientPrincipal
        {
            public string IdentityProvider { get; set; }
            public string UserId { get; set; }
            public string UserDetails { get; set; }
            public IEnumerable<string> UserRoles { get; set; }
        }

        [FunctionName("UserInfoGet")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "userinfo")] HttpRequest req,
            ILogger log)
        {
            bool useMockData;
            if (!bool.TryParse(System.Environment.GetEnvironmentVariable("UseMockData"), out useMockData))
            {
                useMockData = false;
            }

            if (useMockData)
            {
                log.LogInformation("Returning mock data: Hello Benedict Cumberbatch! Your user id: test123");
                return new OkObjectResult($"Hello Benedict Cumberbatch! Your user id: test123");
            }

            var principal = new ClientPrincipal();

            if (req.Headers.TryGetValue("x-ms-client-principal", out var header))
            {
                log.LogInformation("Found 'x-ms-client-principal' header: " + header);

                var data = header[0];
                var decoded = Convert.FromBase64String(data);
                var json = Encoding.ASCII.GetString(decoded);
                principal = JsonSerializer.Deserialize<ClientPrincipal>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                string responseMessage = $"Hello {principal.UserDetails}! Your user id: {principal.UserId}";
                return new OkObjectResult(responseMessage);
            }

            log.LogError("The request doesn't have 'x-ms-client-principal' header. Cannot verify the user identity.");
            return new NotFoundObjectResult("The request doesn't have 'x-ms-client-principal' header. Cannot verify the user identity.");
        }
    }
}
