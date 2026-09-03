using Inventory.APP;
using Inventory.SharedUI;
using Inventory.SharedUI.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
builder.Configuration.AddJsonFile($"appsettings.{builder.HostEnvironment.Environment}.json", optional: true, reloadOnChange: false);

var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"];
var apiHostOverride = builder.Configuration["ApiSettings:Host"];
var apiPort = builder.Configuration.GetValue<int?>("ApiSettings:Port") ?? 5130;
var apiHttpsPort = builder.Configuration.GetValue<int?>("ApiSettings:HttpsPort") ?? 5131;
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
        // La API sirve esta misma app (mismo origen): no hay que cambiar nada. Es el
        // caso normal en el teléfono, y el que evita tener que confiar en un segundo
        // certificado y lidiar con CORS.
        apiBaseUrl = appOrigin.GetLeftPart(UriPartial.Authority);
    }
    else
    {
        // La app viene del servidor de desarrollo (puerto 5114), así que la API está
        // en otro puerto. Se elige el que coincide con el esquema de esta página,
        // porque un sitio https no puede llamar a http (contenido mixto).
        apiBaseUrl = $"{appOrigin.Scheme}://{resolvedHost}:{(appIsHttps ? apiHttpsPort : apiPort)}";
    }
}
else if (appIsHttps && apiBaseUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
{
    // BaseUrl quedó fijada en http mientras la app se sirve por https: se corrige
    // al endpoint HTTPS de la API en lugar de dejar que el navegador bloquee todo.
    var configured = new Uri(apiBaseUrl);
    var upgraded = $"https://{configured.Host}:{apiHttpsPort}{configured.AbsolutePath.TrimEnd('/')}";
    Console.WriteLine($"[Inventario] ApiSettings:BaseUrl usa http ({apiBaseUrl}) pero la app se sirve por https; se usará {upgraded}.");
    apiBaseUrl = upgraded;
}

Console.WriteLine($"[Inventario] API base URL: {apiBaseUrl}");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl.TrimEnd('/') + "/") });
builder.Services.AddInventoryApiServices();

await builder.Build().RunAsync();
