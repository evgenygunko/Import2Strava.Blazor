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

        Task UnlinkStravaAppAsync();

        Task<UserInfoModel> GetUserInfoAsync();

        Task<string> GetApiVersionAsync();
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

        public async Task UnlinkStravaAppAsync()
        {
            var response = await _httpClient.PostAsync(new Uri($"/api/disconnect", UriKind.Relative), null);

            response.EnsureSuccessStatusCode();
        }

        public async Task<string> GetApiVersionAsync()
        {
            return await _httpClient.GetStringAsync(new Uri("/api/version", UriKind.Relative));
        }

        public async Task<UserInfoModel> GetUserInfoAsync()
        {
            HttpResponseMessage response = await _httpClient.GetAsync(new Uri($"/api/userinfo/", UriKind.Relative));

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();

            UserInfoModel userInfo = JsonConvert.DeserializeObject<UserInfoModel>(responseBody);
            return userInfo;
        }
    }
}
