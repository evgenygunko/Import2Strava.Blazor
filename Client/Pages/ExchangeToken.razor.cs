using System;
using System.Threading.Tasks;
using Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace Client.Pages
{
    public partial class ExchangeToken
    {
        [Inject]
        public NavigationManager NavManager { get; set; }

        [Inject]
        public IDataService DataService { get; set; }

        [Inject]
        public ILogger<ExchangeToken> Logger { get; set; }

        public string Error { get; set; }

        public string AuthorizationCode { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var uriBuilder = new UriBuilder(NavManager.Uri);
            var queryStringParams = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);

            AuthorizationCode = queryStringParams["code"];
            Logger.LogInformation($"Received authorization code='{AuthorizationCode}'");

            try
            {
                await DataService.ConnectStravaAppAsync(AuthorizationCode);
                Logger.LogInformation($"Successfully connected Strava app");

                NavManager.NavigateTo("/");
            }
            catch (Exception ex)
            {
                Error = "An error occurred: " + ex;
            }
        }
    }
}
