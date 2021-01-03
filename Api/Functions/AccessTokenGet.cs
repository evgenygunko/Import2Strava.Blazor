using System;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "accesstoken/{authorizationCode}")]
            HttpRequest req,
            string authorizationCode,
            ILogger logger)
        {
            string accessToken = await PerformCodeExchangeAsync(authorizationCode, logger);
            return new OkObjectResult(accessToken);
        }

        private async Task<string> PerformCodeExchangeAsync(string code, ILogger logger)
        {
            logger.LogInformation("Exchanging code for tokens...");

            string accessToken = null;

            ExchangeTokenModel exchangeTokenModel = new ExchangeTokenModel()
            {
                ClientId = _configuration["Strava:ClientId"],
                ClientSecret = _configuration["Strava:ClientSecret"],
                AuthorizationCode = code
            };
            var json = JsonConvert.SerializeObject(exchangeTokenModel);

            // builds the  request
            HttpResponseMessage response = await _httpClient.PostAsync(new Uri($"https://www.strava.com/oauth/token"), new StringContent(json, Encoding.UTF8, "application/json"));

            response.EnsureSuccessStatusCode();

            // reads response body
            string responseText = await response.Content.ReadAsStringAsync();
            logger.LogTrace(responseText);

            // converts to dictionary
            var data = (JObject)JsonConvert.DeserializeObject(responseText);
            accessToken = data["access_token"].Value<string>();
            string refreshToken = data["refresh_token"].Value<string>();

            DateTime jan1970 = Convert.ToDateTime("1970-01-01T00:00:00Z", CultureInfo.InvariantCulture);
            DateTime expiresAt = jan1970.AddSeconds(data["expires_at"].Value<long>());

            logger.LogInformation("The tokens have been acquired.");

            return accessToken;
        }

    }
}