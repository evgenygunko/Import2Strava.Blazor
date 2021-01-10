using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;

namespace Client.Components
{
    public partial class LinkStravaAccount
    {
        [Inject]
        private IConfiguration Configuration { get; set; }

        public string AuthorizationUrl
        {
            get
            {
                string authorizationUri = "http://www.strava.com/oauth/authorize";
                return authorizationUri + $"?client_id={Configuration["OAuthClientId"]}&response_type=code&redirect_uri={Configuration["OAuthRedirectUri"]}&approval_prompt=auto&scope=read";
            }
        }
    }
}
