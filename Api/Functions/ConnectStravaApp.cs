//using System;
//using System.Net.Http;
//using System.Text;
//using System.Threading.Tasks;
//using Api.Helpers;
//using Api.Models;
//using Api.Models.Data;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Azure.WebJobs;
//using Microsoft.Azure.WebJobs.Extensions.Http;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Logging;
//using Newtonsoft.Json;

//namespace Api.Functions
//{
//    public class ConnectStravaApp
//    {
//        private readonly HttpClient _httpClient;
//        private readonly IConfiguration _configuration;

//        public ConnectStravaApp(HttpClient httpClient, IConfiguration configuration)
//        {
//            _httpClient = httpClient;
//            _configuration = configuration;
//        }

//        [FunctionName("ConnectStravaApp")]
//        [return: Table("LinkedAccounts")]
//        public async Task<LinkedAccount> RunAsync(
//            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "connect")]
//            [FromBody] Shared.Models.ConnectStravaApp connectStravaApp,
//            HttpRequest req,
//            ILogger logger)
//        {
//            logger.LogInformation($"ConnectStravaApp function is called with AuthorizationCode: '{connectStravaApp.AuthorizationCode}'");

//            ClientPrincipal clientPrincipal = AuthenticationHelper.GetClientPrincipal(req, logger);

//            AccessTokenModel accessTokenModel = await PerformCodeExchangeAsync(connectStravaApp.AuthorizationCode, clientPrincipal, logger);

//            // TODO: save token in the database
//            LinkedAccount linkedAccount = new LinkedAccount()
//            {
//                UserId = clientPrincipal.UserId,
//                UserDetails = clientPrincipal.UserDetails,
//                IdentityProvider = clientPrincipal.IdentityProvider,
//                StravaAccountId = accessTokenModel.Athlete.Id,
//                FirstName = accessTokenModel.Athlete.FirstName,
//                LastName = accessTokenModel.Athlete.LastName,
//                Profile = accessTokenModel.Athlete.Profile,
//                TokenType = accessTokenModel.TokenType,
//                AccessToken = accessTokenModel.AccessToken,
//                ExpiresAt = accessTokenModel.ExpiresAt,
//                RefreshToken = accessTokenModel.RefreshToken,
//            };

//            logger.LogInformation($"Saving access token for user: '{linkedAccount.UserId}' '{linkedAccount.UserDetails}'. Strava user:'{linkedAccount.StravaAccountId}' {linkedAccount.FirstName} {linkedAccount.LastName}");
//            linkedAccount.PartitionKey = "LinkedAccounts";
//            linkedAccount.RowKey = linkedAccount.UserId;

//            return linkedAccount;
//        }

//        private async Task<AccessTokenModel> PerformCodeExchangeAsync(string code, ClientPrincipal clientPrincipal, ILogger logger)
//        {
//            logger.LogInformation($"Exchanging authorization code '{code}' for tokens for client {clientPrincipal.UserId} {clientPrincipal.UserDetails}...");

//            ExchangeTokenModel exchangeTokenModel = new ExchangeTokenModel()
//            {
//                ClientId = _configuration["Strava:ClientId"],
//                ClientSecret = _configuration["Strava:ClientSecret"],
//                AuthorizationCode = code
//            };
//            var json = JsonConvert.SerializeObject(exchangeTokenModel);

//            HttpResponseMessage response = await _httpClient.PostAsync(new Uri($"https://www.strava.com/oauth/token"), new StringContent(json, Encoding.UTF8, "application/json"));

//            response.EnsureSuccessStatusCode();

//            string responseBody = await response.Content.ReadAsStringAsync();
//            logger.LogInformation($"Received token response from Strava: '{responseBody}'");

//            AccessTokenModel accessTokenModel = JsonConvert.DeserializeObject<AccessTokenModel>(responseBody);
//            logger.LogInformation($"Parsed Access token: '{accessTokenModel.AccessToken}', expires at '{accessTokenModel.ExpiresAt}'");

//            return accessTokenModel;
//        }
//    }
//}