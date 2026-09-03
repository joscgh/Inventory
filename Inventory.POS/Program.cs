using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Inventory.POS;
using Inventory.SharedUI;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
builder.Configuration.AddJsonFile($"appsettings.{builder.HostEnvironment.Environment}.json", optional: true, reloadOnChange: false);

var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"];
var apiHostOverride = builder.Configuration["ApiSettings:Host"];
var apiPort = builder.Configuration.GetValue<int?>("ApiSettings:Port") ?? 5130;
var apiHttpsPort = builder.Configuration.GetValue<int?>("ApiSettings:HttpsPort") ?? 5114;
var useCurrentHost = builder.Configuration.GetValue<bool?>("ApiSettings:UseCurrentHost") ?? true;

var appOrigin = new Uri(builder.HostEnvironment.BaseAddress);
var appIsHttps = string.Equals(appOrigin.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);

if (string.IsNullOrWhiteSpace(apiBaseUrl))
{
	var resolvedHost = useCurrentHost || string.IsNullOrWhiteSpace(apiHostOverride)
		? appOrigin.Host
		: apiHostOverride;

	if (resolvedHost == appOrigin.Host && (appOrigin.Port == apiPort || appOrigin.Port == apiHttpsPort))
	{
		apiBaseUrl = appOrigin.GetLeftPart(UriPartial.Authority);
	}
	else
	{
		apiBaseUrl = $"{appOrigin.Scheme}://{resolvedHost}:{(appIsHttps ? apiHttpsPort : apiPort)}";
	}
}
else if (appIsHttps && apiBaseUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
{
	var configured = new Uri(apiBaseUrl);
	var upgraded = $"https://{configured.Host}:{apiHttpsPort}{configured.AbsolutePath.TrimEnd('/')}";
	Console.WriteLine($"[POS] ApiSettings:BaseUrl usa http ({apiBaseUrl}) pero la app se sirve por https; se usará {upgraded}.");
	apiBaseUrl = upgraded;
}

Console.WriteLine($"[POS] API base URL: {apiBaseUrl}");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl.TrimEnd('/') + "/") });
builder.Services.AddInventoryApiServices();

await builder.Build().RunAsync();
