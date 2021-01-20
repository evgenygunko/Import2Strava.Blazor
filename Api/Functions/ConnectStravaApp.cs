using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Api.Models;
using Api.Models.Data;
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

namespace Api.Functions
{
    public class ConnectStravaApp
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IAuth0Authenticator _auth0Authenticator;

        public ConnectStravaApp(HttpClient httpClient, IConfiguration configuration, IAuth0Authenticator auth0Authenticator)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _auth0Authenticator = auth0Authenticator;
        }

        [FunctionName("ConnectStravaApp")]
        public async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "connect")]
            [FromBody] Shared.Models.ConnectStravaApp connectStravaApp,
            HttpRequest request,
            [Table("LinkedAccounts")] CloudTable table,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            (ClaimsPrincipal user, SecurityToken _) = await _auth0Authenticator.AuthenticateAsync(request, logger, cancellationToken);

            string userId = _auth0Authenticator.GetUserId(user);
            logger.LogInformation($"User authenticated, user id='{userId}'");

            // Hit the auth0 user_info API and see what we get back about this user
            var authorizationHeader = AuthenticationHeaderValue.Parse(request.Headers["Authorization"]);
            Auth0UserInfo userInfo = await GetAuth0UserinfoAsync(authorizationHeader, logger, cancellationToken);
            logger.LogInformation($"Got user info from auth0: '{userInfo}'");

            AccessTokenModel stravaAccessToken = await PerformCodeExchangeAsync(connectStravaApp.AuthorizationCode, userInfo, logger, cancellationToken);

            // Save token in the table storage
            LinkedAccount linkedAccount = new LinkedAccount()
            {
                IdpUserId = userInfo.UserId,
                IdpUserName = userInfo.Name,
                StravaAccountId = stravaAccessToken.Athlete.Id,
                FirstName = stravaAccessToken.Athlete.FirstName,
                LastName = stravaAccessToken.Athlete.LastName,
                Profile = stravaAccessToken.Athlete.Profile,
                TokenType = stravaAccessToken.TokenType,
                AccessToken = stravaAccessToken.AccessToken,
                ExpiresAt = stravaAccessToken.ExpiresAt,
                RefreshToken = stravaAccessToken.RefreshToken,
            };

            logger.LogInformation($"Saving access token for user: '{linkedAccount.IdpUserId}' '{linkedAccount.IdpUserName}'. Strava user:'{linkedAccount.StravaAccountId}' {linkedAccount.FirstName} {linkedAccount.LastName}");
            linkedAccount.PartitionKey = "LinkedAccounts";
            linkedAccount.RowKey = linkedAccount.IdpUserId;
            linkedAccount.ETag = "*";

            TableOperation operation = TableOperation.InsertOrMerge(linkedAccount);
            await table.ExecuteAsync(operation);

            return new OkResult();
        }

        private async Task<AccessTokenModel> PerformCodeExchangeAsync(string code, Auth0UserInfo auth0UserInfo, ILogger logger, CancellationToken cancellationToken)
        {
            logger.LogInformation($"Exchanging authorization code '{code}' for tokens for client {auth0UserInfo.UserId} {auth0UserInfo.Name}...");

            ExchangeTokenModel exchangeTokenModel = new ExchangeTokenModel()
            {
                ClientId = _configuration["Strava:ClientId"],
                ClientSecret = _configuration["Strava:ClientSecret"],
                AuthorizationCode = code
            };
            var json = JsonConvert.SerializeObject(exchangeTokenModel);

            HttpResponseMessage response = await _httpClient.PostAsync(new Uri($"https://www.strava.com/oauth/token"), new StringContent(json, Encoding.UTF8, "application/json"), cancellationToken);

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            logger.LogInformation($"Received token response from Strava: '{responseBody}'");

            AccessTokenModel accessTokenModel = JsonConvert.DeserializeObject<AccessTokenModel>(responseBody);
            logger.LogInformation($"Parsed Access token: '{accessTokenModel.AccessToken}', expires at '{accessTokenModel.ExpiresAt}'");

            return accessTokenModel;
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