using Inventory.APP;
using Inventory.RazorLib.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5130/") });
builder.Services.AddScoped<ItemApiService>();
builder.Services.AddScoped<CurrencyApiService>();
builder.Services.AddScoped<CategoryApiService>();

await builder.Build().RunAsync();
