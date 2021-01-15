using System.Threading.Tasks;
using Client.Services;
using Microsoft.AspNetCore.Components;

namespace Client.Pages
{
    public partial class ApiVersion
    {
        [Inject]
        public IDataService DataService { get; set; }

        public string Version { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Version = await DataService.GetApiVersionAsync();
        }
    }
}
