using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Client.Services
{
    public interface IClientService
    {
        Task<string> GetUserDetailsAsync();
    }

    public class ClientService : IClientService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ClientService> _logger;

        public ClientService(
            HttpClient httpClient,
            ILogger<ClientService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<string> GetUserDetailsAsync()
        {
            return await _httpClient.GetStringAsync(new Uri("/api/userinfo", UriKind.Relative));
        }
    }
}
