using System.Threading.Tasks;
using Client.Services;
using Microsoft.AspNetCore.Components;
using Shared.Models;

namespace Client.Pages
{
    public partial class Index
    {
        [Inject]
        public IDataService DataService { get; set; }

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
