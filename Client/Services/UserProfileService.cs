using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Client.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Client.Services
{
    public interface IUserProfileService
    {
        Task<AthleteModel> GetProfileAsync();
    }

    public class UserProfileService : IUserProfileService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UserProfileService> _logger;
        private readonly IAuthenticationService _authenticationService;

        public UserProfileService(
            HttpClient httpClient,
            ILogger<UserProfileService> logger,
            IAuthenticationService authenticationService)
        {
            _httpClient = httpClient;
            _logger = logger;
            _authenticationService = authenticationService;
        }

        public async Task<AthleteModel> GetProfileAsync()
        {
            string accessToken = _authenticationService.GetAccessToken();
            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogWarning("Could not get access token, the operation is canceled.");
                return null;
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            HttpResponseMessage response = await _httpClient.GetAsync(new Uri("/api/v3/athlete", UriKind.Relative));

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();

            AthleteModel athleteModel = JsonConvert.DeserializeObject<AthleteModel>(responseBody);
            return athleteModel;
        }
    }
}
