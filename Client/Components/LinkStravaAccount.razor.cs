using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;

namespace Client.Components
{
    public partial class LinkStravaAccount
    {
        [Inject]
        public IConfiguration Configuration { get; set; }

        [Inject]
        public NavigationManager NavManager { get; set; }

        public string AuthorizationUrl
        {
            get
            {
                // Will be something like "https://witty-dune-0fc04b403.azurestaticapps.net/exchange_token"
                string redirectUri = NavManager.ToAbsoluteUri("exchange_token").ToString();

                string authorizationUri = "http://www.strava.com/oauth/authorize";
                return authorizationUri + $"?client_id={Configuration["OAuthClientId"]}&response_type=code&redirect_uri={redirectUri}&approval_prompt=auto&scope=read";
            }
        }
    }
}
