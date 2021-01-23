using Newtonsoft.Json;

namespace Api.Models
{
    public class RefreshTokenModel
    {
        [JsonProperty("client_id")]
        public string ClientId { get; set; }

        [JsonProperty("client_secret")]
        public string ClientSecret { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("grant_type")]
        public string GrantType => "refresh_token";
    }
}
