using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
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

namespace Api.Functions
{
    public class DisconnectStravaApp
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IAuth0Authenticator _auth0Authenticator;
        private readonly ILinkedAccountService _linkedAccountService;

        public DisconnectStravaApp(HttpClient httpClient, IConfiguration configuration, IAuth0Authenticator auth0Authenticator, ILinkedAccountService linkedAccountService)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _auth0Authenticator = auth0Authenticator;
            _linkedAccountService = linkedAccountService;
        }

        [FunctionName("DisconnectStravaApp")]
        public async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "disconnect")]
            HttpRequest request,
            [Table("LinkedAccounts")] CloudTable table,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            (ClaimsPrincipal user, SecurityToken _) = await _auth0Authenticator.AuthenticateAsync(request, logger, cancellationToken);

            string userId = _auth0Authenticator.GetUserId(user);
            logger.LogInformation($"User authenticated, user id='{userId}'");

            try
            {
                LinkedAccount linkedAccount = await _linkedAccountService.GetLinkedAccountAsync(userId, table, logger);

                // De-authorize Strava account
                await DeauthorizeAsync(linkedAccount, logger, cancellationToken);

                // Delete linked account from table storage
                TableOperation operation = TableOperation.Delete(linkedAccount);
                await table.ExecuteAsync(operation);

                // todo: delete workout files from blob storage
                return new OkResult();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while trying to unlink Strava account");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        private async Task DeauthorizeAsync(LinkedAccount linkedAccount, ILogger logger, CancellationToken cancellationToken)
        {
            logger.LogInformation($"Calling deauthorize endpoint for user: '{linkedAccount.IdpUserId}' '{linkedAccount.IdpUserName}'. Strava user:'{linkedAccount.StravaAccountId}' {linkedAccount.FirstName} {linkedAccount.LastName}");

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("https://www.strava.com/oauth/deauthorize"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", linkedAccount.AccessToken);

            HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            logger.LogInformation($"Received token response from Strava: '{responseBody}'");
        }
    }
}