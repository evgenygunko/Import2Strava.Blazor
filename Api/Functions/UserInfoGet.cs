using Api.Helpers;
using Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Api.Functions
{
    public static class UserInfoGet
    {
        [FunctionName("UserInfoGet")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "userinfo")] HttpRequest req,
            ILogger log)
        {
            ClientPrincipal principal = AuthenticationHelper.GetClientPrincipal(req, log);

            string responseMessage = $"Hello {principal.UserDetails}! Your user id: {principal.UserId}, identity provider: {principal.IdentityProvider}";
            return new OkObjectResult(responseMessage);
        }
    }
}
