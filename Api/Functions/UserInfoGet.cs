using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Api.Models;
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
using Shared.Models;

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

                // todo: read the token from table, make a request to Strava and get Athlete profile
                //var authorizationHeader = AuthenticationHeaderValue.Parse(request.Headers["Authorization"]);
                //AthleteModel athlete = await GetAthleteAsync(authorizationHeader, logger, cancellationToken);
                AthleteModel athlete = new AthleteModel()
                {
                    Id = 123,
                    FirstName = "Benedict",
                    LastName = "Cumberbatch",
                    Profile = null
                };

                logger.LogInformation($"Got athlete info from Strava: '{athlete}'");

                UserInfoModel userInfoModel = new UserInfoModel();
                userInfoModel.FirstName = athlete.FirstName;
                userInfoModel.LastName = athlete.LastName;
                userInfoModel.Country = athlete.Country;
                userInfoModel.PictureUrl = athlete.Profile;
                userInfoModel.IsStravaAccountLinked = true;

                return new OkObjectResult(userInfoModel);
            }
            catch (AuthException)
            {
                return new UnauthorizedResult();
            }
        }

        private async Task<AthleteModel> GetAthleteAsync(AuthenticationHeaderValue token, ILogger logger, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri("https://www.strava.com/api/athlete/"));
            request.Headers.Authorization = token;

            HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

            string responseBody = await response.Content.ReadAsStringAsync();
            logger.LogInformation($"Received response from Strava: '{responseBody}'");

            AthleteModel athleteModel = JsonConvert.DeserializeObject<AthleteModel>(responseBody);
            return athleteModel;
        }
    }
}
