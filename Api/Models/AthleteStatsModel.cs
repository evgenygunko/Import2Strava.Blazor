using Newtonsoft.Json;

namespace Api.Models
{
    public class AthleteStatsModel
    {
        [JsonProperty("all_run_totals")]
        public ActivityTotal AllRunsTotals { get; set; }

        [JsonProperty("all_swim_totals")]
        public ActivityTotal AllSwimsTotals { get; set; }

        [JsonProperty("all_ride_totals")]
        public ActivityTotal AllRidesTotals { get; set; }
    }
}
