using Newtonsoft.Json;

namespace Api.Models
{
    public class ActivityTotal
    {
        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("distance")]
        public float Distance { get; set; }

        [JsonProperty("moving_time")]
        public int MovingTime { get; set; }

        [JsonProperty("elapsed_time")]
        public int ElapsedTime { get; set; }

        [JsonProperty("elevation_gain")]
        public float ElevationGain { get; set; }

        [JsonProperty("achievement_count")]
        public int AchievementCount { get; set; }
    }
}
