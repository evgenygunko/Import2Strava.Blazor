using System;
using Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Client.Pages
{
    public partial class ExchangeToken
    {
        [Inject]
        private NavigationManager NavManager { get; set; }

        [Inject]
        private IAuthenticationService AuthenticationService { get; set; }

        public string AuthorizationCode { get; set; }

        public string AccessToken { get; set; }

        protected override void OnInitialized()
        {
            var uriBuilder = new UriBuilder(NavManager.Uri);
            var queryStringParams = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
            AuthorizationCode = queryStringParams["code"];

            //NavManager.NavigateTo("/");
        }

        private void GetAccessToken(MouseEventArgs e)
        {
            InvokeAsync(async () =>
            {
                AccessToken = await AuthenticationService.AuthenticateAsync(AuthorizationCode);
                StateHasChanged();
            });
        }
    }
}
