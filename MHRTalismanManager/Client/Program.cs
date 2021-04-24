using System;
using System.Net.Http;
using System.Threading.Tasks;
using BlazorApplicationInsights;
using Blazored.LocalStorage;
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace MHRTalismanManager.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddBlazorise(options => { options.ChangeTextOnKeyPress = true; })
                   .AddBootstrapProviders()
                   .AddFontAwesomeIcons();

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddBlazorApplicationInsights(async applicationInsights =>
                                                          {
                                                              await applicationInsights.SetInstrumentationKey("e79f4453-4a5d-4d3f-a3ce-ecc9222e2902");
                                                              await applicationInsights.LoadAppInsights();
                                                          });

            await builder.Build()
                         .RunAsync();
        }
    }
}
