using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Api.Models;
using Api.Models.Data;
using Api.Models.Exceptions;
using Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
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
        private readonly ILinkedAccountService _linkedAccountService;

        public UserInfoGet(HttpClient httpClient, IConfiguration configuration, IAuth0Authenticator auth0Authenticator, ILinkedAccountService linkedAccountService)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _auth0Authenticator = auth0Authenticator;
            _linkedAccountService = linkedAccountService;
        }

        [FunctionName("UserInfoGet")]
        public async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "userinfo")]
            HttpRequest request,
            [Table("LinkedAccounts")] CloudTable table,
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

                UserInfoModel userInfo = new UserInfoModel();

                LinkedAccount linkedAccount = await _linkedAccountService.GetLinkedAccountAsync(userId, table, logger);
                if (linkedAccount != null)
                {
                    // validate that the access token has not expired and renew if needed
                    string accessToken = await GetValidStravaAccessTokenAsync(linkedAccount, table, logger, cancellationToken);

                    if (accessToken != null)
                    {
                        AthleteModel athlete = await GetAthleteAsync(accessToken, linkedAccount, table, logger, cancellationToken);
                        if (athlete != null)
                        {
                            logger.LogInformation($"Got athlete info from Strava: '{athlete}'");

                            userInfo.FirstName = athlete.FirstName;
                            userInfo.LastName = athlete.LastName;
                            userInfo.Country = athlete.Country;
                            userInfo.City = athlete.City;
                            userInfo.PictureUrl = athlete.Profile;

                            userInfo.IsStravaAccountLinked = true;
                        }

                        AthleteStatsModel stats = await GetAthleteStatsAsync(accessToken, linkedAccount, logger, cancellationToken);
                        if (stats != null)
                        {
                            logger.LogInformation($"Got athlete stats from Strava: '{stats}'");

                            userInfo.Runs = stats.AllRunsTotals.Count;
                            userInfo.Swims = stats.AllSwimsTotals.Count;
                            userInfo.Rides = stats.AllRidesTotals.Count;
                        }
                    }
                }

                return new OkObjectResult(userInfo);
            }
            catch (AuthException)
            {
                return new UnauthorizedResult();
            }
        }

        #region Private Methods

        private async Task<string> GetValidStravaAccessTokenAsync(LinkedAccount linkedAccount, CloudTable table, ILogger logger, CancellationToken cancellationToken)
        {
            if (linkedAccount.ExpiresAt > DateTime.Now)
            {
                return linkedAccount.AccessToken;
            }

            try
            {
                logger.LogInformation($"Access token expired at '{linkedAccount.ExpiresAt}', trying to refresh...");

                RefreshTokenModel refreshTokenModel = new RefreshTokenModel()
                {
                    ClientId = _configuration["Strava:ClientId"],
                    ClientSecret = _configuration["Strava:ClientSecret"],
                    RefreshToken = linkedAccount.RefreshToken
                };
                var json = JsonConvert.SerializeObject(refreshTokenModel);

                HttpResponseMessage response = await _httpClient.PostAsync(new Uri("https://www.strava.com/api/v3/oauth/token"), new StringContent(json, Encoding.UTF8, "application/json"), cancellationToken);

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                logger.LogInformation($"Received token response from Strava: '{responseBody}'");

                AccessTokenModel accessTokenModel = JsonConvert.DeserializeObject<AccessTokenModel>(responseBody);
                logger.LogInformation($"Parsed Access token: '{accessTokenModel.AccessToken}', expires at '{accessTokenModel.ExpiresAt}'");

                // Update tokens in the table storage
                logger.LogInformation($"Saving access token for user: '{linkedAccount.IdpUserId}' '{linkedAccount.IdpUserName}'. Strava user:'{linkedAccount.StravaAccountId}' {linkedAccount.FirstName} {linkedAccount.LastName}");
                linkedAccount.TokenType = accessTokenModel.TokenType;
                linkedAccount.AccessToken = accessTokenModel.AccessToken;
                linkedAccount.ExpiresAt = accessTokenModel.ExpiresAt;
                linkedAccount.RefreshToken = accessTokenModel.RefreshToken;

                TableOperation operation = TableOperation.Merge(linkedAccount);
                await table.ExecuteAsync(operation);

                return accessTokenModel.AccessToken;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while requesting athlete info from Strava");
                return null;
            }
        }

        private async Task<AthleteModel> GetAthleteAsync(string token, LinkedAccount linkedAccount, CloudTable table, ILogger logger, CancellationToken cancellationToken)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, new Uri("https://www.strava.com/api/v3/athlete"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                logger.LogInformation($"Received response from Strava: '{responseBody}'");

                AthleteModel athleteModel = JsonConvert.DeserializeObject<AthleteModel>(responseBody);

                // Update linked account in the table storage if the user profile changed
                if (linkedAccount.StravaAccountId != athleteModel.Id
                    || linkedAccount.FirstName != athleteModel.FirstName
                    || linkedAccount.LastName != athleteModel.LastName)
                {
                    logger.LogInformation($"Updating Strava account for user: '{linkedAccount.IdpUserId}' '{linkedAccount.IdpUserName}'. Strava user:'{linkedAccount.StravaAccountId}' {linkedAccount.FirstName} {linkedAccount.LastName}");
                    linkedAccount.StravaAccountId = athleteModel.Id;
                    linkedAccount.FirstName = athleteModel.FirstName;
                    linkedAccount.LastName = athleteModel.LastName;

                    TableOperation operation = TableOperation.Merge(linkedAccount);
                    await table.ExecuteAsync(operation);
                }

                return athleteModel;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while requesting athlete info from Strava");
                return null;
            }
        }

        private async Task<AthleteStatsModel> GetAthleteStatsAsync(string token, LinkedAccount linkedAccount, ILogger logger, CancellationToken cancellationToken)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, new Uri("https://" + $"www.strava.com/api/v3/athletes/{linkedAccount.StravaAccountId}/stats"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                logger.LogInformation($"Received response from Strava: '{responseBody}'");

                AthleteStatsModel statsModel = JsonConvert.DeserializeObject<AthleteStatsModel>(responseBody);
                return statsModel;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while requesting athlete info from Strava");
                return null;
            }
        }

        #endregion
    }
}
