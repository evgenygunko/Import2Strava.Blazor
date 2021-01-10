using System.Threading.Tasks;
using Client.Models;
using Client.Services;
using Microsoft.AspNetCore.Components;

namespace Client.Pages
{
    public partial class Index
    {
        [Inject]
        private IDataService DataService { get; set; }

        public AthleteModel Athlete { get; set; }

        public bool IsAuthenticated { get; set; }

        protected override async Task OnInitializedAsync()
        {
            if (IsAuthenticated)
            {
                Athlete = await DataService.GetAthleteAsync();
            }
        }
    }
}
