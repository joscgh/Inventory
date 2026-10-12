using System.Globalization;
using Inventory.Restaurant;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo("es-VE");
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("es-VE");
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var appOrigin = new Uri(builder.HostEnvironment.BaseAddress);
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"];
if (string.IsNullOrWhiteSpace(apiBaseUrl))
{
    var useHttps = appOrigin.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);
    var port = builder.Configuration.GetValue<int?>(useHttps ? "ApiSettings:HttpsPort" : "ApiSettings:Port")
        ?? (useHttps ? 5114 : 5130);
    apiBaseUrl = $"{appOrigin.Scheme}://{appOrigin.Host}:{port}";
}

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(apiBaseUrl.TrimEnd('/') + "/") });
await builder.Build().RunAsync();