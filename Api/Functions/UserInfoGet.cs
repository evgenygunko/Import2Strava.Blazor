using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Api.Models.Data;
using Api.Models.Exceptions;
using Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

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
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "userinfo")]
            HttpRequest request,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            try
            {
                // The "user" returned here is an actual ClaimsPrincipal with the claims that were in the access_token.
                // The "token" is a SecurityToken that can be used to invoke services on the part of the user. E.g., create a Google Calendar event on the user's calendar.
                (ClaimsPrincipal user, SecurityToken token) = await _auth0Authenticator.AuthenticateAsync(request, logger, cancellationToken);

                string userId = _auth0Authenticator.GetUserId(user);
                logger.LogInformation($"User authenticated, user id='{userId}'");

                // Hit the auth0 user_info API and see what we get back about this user
                var authorizationHeader = AuthenticationHeaderValue.Parse(request.Headers["Authorization"]);
                Auth0UserInfo auth0UserInfo = await GetAuth0UserinfoAsync(authorizationHeader, logger, cancellationToken);
                logger.LogInformation($"Got user info from auth0: '{auth0UserInfo}'");

                return new OkObjectResult(auth0UserInfo);
            }
            catch (AuthException)
            {
                return new UnauthorizedResult();
            }
        }

        private async Task<Auth0UserInfo> GetAuth0UserinfoAsync(AuthenticationHeaderValue token, ILogger logger, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://{_configuration["Auth0Domain"]}/userinfo");
            request.Headers.Authorization = token;

            HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

            string responseBody = await response.Content.ReadAsStringAsync();
            logger.LogInformation($"Received token response from Auth0: '{responseBody}'");

            Auth0UserInfo userInfo = JsonConvert.DeserializeObject<Auth0UserInfo>(responseBody);
            return userInfo;
        }
    }
}
