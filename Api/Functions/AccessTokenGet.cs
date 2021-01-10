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
    public class AccessTokenGet
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AccessTokenGet(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        [FunctionName("AccessTokenGet")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "accesstoken/{authorizationCode}")]
            HttpRequest req,
            string authorizationCode,
            ILogger logger)
        {
            ClientPrincipal clientPrincipal = AuthenticationHelper.GetClientPrincipal(req, logger);

            string accessToken = await PerformCodeExchangeAsync(authorizationCode, clientPrincipal, logger);
            return new OkObjectResult(accessToken);
        }

        private async Task<string> PerformCodeExchangeAsync(string code, ClientPrincipal clientPrincipal, ILogger logger)
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

            return accessTokenModel.AccessToken;
        }

    }
}