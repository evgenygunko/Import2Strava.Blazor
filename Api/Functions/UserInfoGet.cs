using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Api.Models.Data;
using Api.Models.Exceptions;
using Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Api.Functions
{
    public class UserInfoGet
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IAuth0Authenticator _auth0Authenticator;

        public UserInfoGet(HttpClient httpClient, IConfiguration configuration, IAuth0Authenticator auth0Authenticator)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _auth0Authenticator = auth0Authenticator;
        }

        [FunctionName("UserInfoGet")]
        public async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "userinfo")] HttpRequestMessage req,
            ILogger log,
            CancellationToken cancellationToken)
        {
            try
            {
                // The "user" returned here is an actual ClaimsPrincipal with the claims that were in the access_token.
                // The "token" is a SecurityToken that can be used to invoke services on the part of the user. E.g., create a Google Calendar event on the user's calendar.
                (ClaimsPrincipal user, SecurityToken token) = await _auth0Authenticator.AuthenticateAsync(req, log, cancellationToken);

                string userId = _auth0Authenticator.GetUserId(user);
                log.LogInformation($"User authenticated, user id='{userId}'");

                // Hit the auth0 user_info API and see what we get back about this user
                Auth0UserInfo auth0UserInfo = await GetAuth0UserinfoAsync(req.Headers.Authorization);
                log.LogInformation($"Got user info from auth0: '{auth0UserInfo}'");

                return new OkObjectResult(auth0UserInfo);
            }
            catch (AuthException)
            {
                return new UnauthorizedResult();
            }
        }

        private async Task<Auth0UserInfo> GetAuth0UserinfoAsync(AuthenticationHeaderValue token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://{_configuration["Auth0Domain"]}/userinfo");
            request.Headers.Authorization = token;

            HttpResponseMessage result = await _httpClient.SendAsync(request);

            return await result.Content.ReadAsAsync<Auth0UserInfo>();
        }
    }
}
