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
        private NavigationManager NavManager { get; set; }

        [Inject]
        private IDataService DataService { get; set; }

        [Inject]
        private ILogger<ExchangeToken> Logger { get; set; }

        public string AuthorizationCode { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var uriBuilder = new UriBuilder(NavManager.Uri);
            var queryStringParams = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);

            AuthorizationCode = queryStringParams["code"];
            Logger.LogInformation($"Received authorization code='{AuthorizationCode}'");

            await DataService.ConnectStravaAppAsync(AuthorizationCode);

            NavManager.NavigateTo("/");
        }
    }
}
