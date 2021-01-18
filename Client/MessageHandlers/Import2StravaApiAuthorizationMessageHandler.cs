using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Configuration;

namespace Client.MessageHandlers
{
    public class Import2StravaApiAuthorizationMessageHandler : AuthorizationMessageHandler
    {
        public Import2StravaApiAuthorizationMessageHandler(
            IAccessTokenProvider provider,
            NavigationManager navigation,
            IConfiguration configuration)
            : base(provider, navigation)
        {
            ConfigureHandler(authorizedUrls: new[] { configuration["API_Prefix"] });
        }
    }
}
