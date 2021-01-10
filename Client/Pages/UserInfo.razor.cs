using System;
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

        public string Error { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                Error = null;
                UserDetails = await ClientService.GetUserDetailsAsync();
            }
            catch (Exception ex)
            {
                Error = "An error occurred: " + ex;
            }
        }
    }
}
