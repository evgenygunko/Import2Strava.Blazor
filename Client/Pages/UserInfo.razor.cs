using System;
using System.Threading.Tasks;
using Client.Services;
using Microsoft.AspNetCore.Components;
using Shared.Models;

namespace Client.Pages
{
    public partial class UserInfo
    {
        [Inject]
        public IDataService DataService { get; set; }

        public UserInfoModel User { get; set; }

        public string Error { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                Error = null;
                User = await DataService.GetUserInfoAsync();
            }
            catch (Exception ex)
            {
                Error = "An error occurred: " + ex;
            }
        }

        private async Task UnlinkStravaAppAsync()
        {
            try
            {
                Error = null;
                await DataService.UnlinkStravaAppAsync();

                User.IsStravaAccountLinked = false;
            }
            catch (Exception ex)
            {
                Error = "An error occurred: " + ex;
            }
        }
    }
}
