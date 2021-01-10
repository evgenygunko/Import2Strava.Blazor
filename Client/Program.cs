using System;
using System.Threading.Tasks;
using AzureStaticWebApps.Blazor.Authentication;
using Client.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddHttpClient<IDataService, DataService>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["API_Prefix"] ?? builder.HostEnvironment.BaseAddress);
            });

            builder.Services.AddStaticWebAppsAuthentication();

            await builder.Build().RunAsync();
        }
    }
}
