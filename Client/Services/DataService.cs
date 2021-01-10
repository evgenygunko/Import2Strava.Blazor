using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Shared.Models;

namespace Client.Services
{
    public interface IDataService
    {
        Task ConnectStravaAppAsync(string authorizationCode);

        Task<AthleteModel> GetAthleteAsync();

        Task<string> GetUserDetailsAsync();
    }

    public class DataService : IDataService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DataService> _logger;

        public DataService(HttpClient httpClient, ILogger<DataService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task ConnectStravaAppAsync(string authorizationCode)
        {
            ConnectStravaApp connectStravaApp = new ConnectStravaApp() { AuthorizationCode = authorizationCode };
            var json = JsonConvert.SerializeObject(connectStravaApp);

            var response = await _httpClient.PostAsync(new Uri($"/api/connect", UriKind.Relative), new StringContent(json, UnicodeEncoding.UTF8, "application/json"));

            response.EnsureSuccessStatusCode();
        }

        public async Task<AthleteModel> GetAthleteAsync()
        {
            HttpResponseMessage response = await _httpClient.GetAsync(new Uri($"/api/athlete/", UriKind.Relative));

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();

            AthleteModel athleteModel = JsonConvert.DeserializeObject<AthleteModel>(responseBody);
            return athleteModel;
        }

        public async Task<string> GetUserDetailsAsync()
        {
            return await _httpClient.GetStringAsync(new Uri("/api/userinfo", UriKind.Relative));
        }
    }
}
