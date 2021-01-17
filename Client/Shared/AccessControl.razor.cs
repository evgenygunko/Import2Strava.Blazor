using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace Client.Shared
{
    public partial class AccessControl
    {
        [Inject]
        public NavigationManager Navigation { get; set; }

        [Inject]
        public SignOutSessionStateManager SignOutManager { get; set; }

        public async Task BeginSignOutAsync()
        {
            await SignOutManager.SetSignOutState();
            Navigation.NavigateTo("authentication/logout");
        }
    }
}
