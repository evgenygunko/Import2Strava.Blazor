using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Api.Helpers;
using Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Api.Functions
{
    public class ConnectStravaApp
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public ConnectStravaApp(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        [FunctionName("ConnectStravaApp")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "connect")]
            [FromBody] Shared.Models.ConnectStravaApp connectStravaApp,
            HttpRequest req,
            ILogger logger)
        {
            ClientPrincipal clientPrincipal = AuthenticationHelper.GetClientPrincipal(req, logger);

            AccessTokenModel accessTokenModel = await PerformCodeExchangeAsync(connectStravaApp.AuthorizationCode, clientPrincipal, logger);

            // TODO: save token in the database
            return new OkResult();
        }

        private async Task<AccessTokenModel> PerformCodeExchangeAsync(string code, ClientPrincipal clientPrincipal, ILogger logger)
        {
            logger.LogInformation($"Exchanging code for tokens for client {clientPrincipal.UserId} {clientPrincipal.UserDetails}...");

            ExchangeTokenModel exchangeTokenModel = new ExchangeTokenModel()
            {
                ClientId = _configuration["Strava:ClientId"],
                ClientSecret = _configuration["Strava:ClientSecret"],
                AuthorizationCode = code
            };
            var json = JsonConvert.SerializeObject(exchangeTokenModel);

            HttpResponseMessage response = await _httpClient.PostAsync(new Uri($"https://www.strava.com/oauth/token"), new StringContent(json, Encoding.UTF8, "application/json"));

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            logger.LogTrace($"Received token response from Strava: '{responseBody}'");

            AccessTokenModel accessTokenModel = JsonConvert.DeserializeObject<AccessTokenModel>(responseBody);
            logger.LogTrace($"Access token: '{accessTokenModel.AccessToken}', expires at '{accessTokenModel.ExpiresAt}'");

            return accessTokenModel;
        }

    }
}