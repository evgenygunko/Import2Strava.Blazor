using System;
using System.Globalization;
using Newtonsoft.Json;

namespace Api.Models
{
    public class AccessTokenModel
    {
        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("expires_at")]
        public long ExpiresAtSeconds { get; set; }

        [JsonProperty("expires_in")]
        public string ExpiresIn { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("athlete")]
        public AthleteModel Athlete { get; set; }

        public DateTime ExpiresAt
        {
            get
            {
                if (ExpiresAtSeconds > 0)
                {
                    DateTime jan1970 = Convert.ToDateTime("1970-01-01T00:00:00Z", CultureInfo.InvariantCulture);
                    DateTime expiresAt = jan1970.AddSeconds(ExpiresAtSeconds);

                    return expiresAt;
                }

                return DateTime.MinValue;
            }
        }
    }
}
