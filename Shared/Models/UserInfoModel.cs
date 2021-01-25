namespace Shared.Models
{
    public class UserInfoModel
    {
        public bool IsStravaAccountLinked { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Country { get; set; }

        public string City { get; set; }

        public string PictureUrl { get; set; }

        public int Runs { get; set; }

        public int Swims { get; set; }

        public int Rides { get; set; }
    }
}
