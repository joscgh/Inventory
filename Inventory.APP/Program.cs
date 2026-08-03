using Inventory.APP;
using Inventory.RazorLib.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
builder.Configuration.AddJsonFile($"appsettings.{builder.HostEnvironment.Environment}.json", optional: true, reloadOnChange: false);

var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"];
if (string.IsNullOrWhiteSpace(apiBaseUrl))
{
    var appOrigin = new Uri(builder.HostEnvironment.BaseAddress);
    apiBaseUrl = $"{appOrigin.Scheme}://{appOrigin.Host}:5130";
}

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl.TrimEnd('/') + "/") });
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
