using System.Threading.Tasks;
using Client.Models;
using Client.Services;
using Microsoft.AspNetCore.Components;

namespace Client.Pages
{
    public partial class Index
    {
        [Inject]
        private IUserProfileService UserProfileService { get; set; }

        public AthleteModel Athlete { get; set; }

        public string Summary { get; set; }

        public bool IsAuthenticated = false;

        protected override async Task OnInitializedAsync()
        {
            if (IsAuthenticated)
            {
                Athlete = await UserProfileService.GetProfileAsync();
            }
        }
    }
}
