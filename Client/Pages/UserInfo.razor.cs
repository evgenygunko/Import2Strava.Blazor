using System.Threading.Tasks;
using Client.Services;
using Microsoft.AspNetCore.Components;

namespace Client.Pages
{
    public partial class UserInfo
    {
        [Inject]
        private IClientService ClientService { get; set; }

        public string UserDetails { get; set; }

        protected override async Task OnInitializedAsync()
        {
            UserDetails = await ClientService.GetUserDetailsAsync();
        }
    }
}
