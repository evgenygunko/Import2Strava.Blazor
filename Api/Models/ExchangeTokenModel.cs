using Newtonsoft.Json;

namespace Api.Models
{
    public class ExchangeTokenModel
    {
        [JsonProperty("client_id")]
        public string ClientId { get; set; }

        [JsonProperty("client_secret")]
        public string ClientSecret { get; set; }

        [JsonProperty("code")]
        public string AuthorizationCode { get; set; }

        [JsonProperty("grant_type")]
        public string GrantType => "authorization_code";
    }
}
