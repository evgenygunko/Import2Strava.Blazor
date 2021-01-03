using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Client.Services
{
    public interface IAuthenticationService
    {
        string GetAccessToken();

        Task<string> AuthenticateAsync(string authorizationCode);
    }

    public class AuthenticationService : IAuthenticationService
    {
        private readonly HttpClient _httpClient;

        private string _accessToken;
        //private DateTime _expiresAt;

        public AuthenticationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public string GetAccessToken()
        {
            return _accessToken;
        }

        public async Task<string> AuthenticateAsync(string authorizationCode)
        {
            string accessToken = await _httpClient.GetStringAsync(new Uri($"/api/accesstoken/{authorizationCode}", UriKind.Relative));
            return accessToken;
        }
    }
}
