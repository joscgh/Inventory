using Inventory.APP;
using Inventory.RazorLib.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Derivamos el host del API del mismo origen desde el que se sirvió la app.
// Así funciona igual en PC (localhost) que desde el teléfono en la red local (ej. 192.168.0.100),
// sin necesidad de codificar la IP a mano.
var appOrigin = new Uri(builder.HostEnvironment.BaseAddress);
var apiBaseAddress = new Uri($"{appOrigin.Scheme}://{appOrigin.Host}:5130/");
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = apiBaseAddress });
builder.Services.AddScoped<ItemApiService>();
builder.Services.AddScoped<CurrencyApiService>();
builder.Services.AddScoped<CategoryApiService>();
builder.Services.AddScoped<TaxApiService>();
builder.Services.AddScoped<AdjustmentApiService>();
builder.Services.AddScoped<CustomerAccountApiService>();
builder.Services.AddScoped<ConsumerCustomerApiService>();
builder.Services.AddScoped<AuthApiService>();
builder.Services.AddScoped<UserStateService>();
builder.Services.AddScoped<NoteApiService>();

await builder.Build().RunAsync();
