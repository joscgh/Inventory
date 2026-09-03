using Inventory.SharedUI;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.Logging;

namespace Inventory.POS.Native;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

#if ANDROID
		builder.ConfigureMauiHandlers(handlers =>
		{
			handlers.AddHandler<BlazorWebView, global::Inventory.Native.CustomBlazorWebViewHandler>();
		});
#endif

		var apiBaseUrl = GetApiBaseUrl();

		builder.Services.AddScoped(sp => new HttpClient(CreateHandler())
		{
			BaseAddress = new Uri(apiBaseUrl.TrimEnd('/') + "/"),
			Timeout = TimeSpan.FromSeconds(20)
		});
		builder.Services.AddInventoryApiServices();

		builder.Services.AddMauiBlazorWebView();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}

	public const string LanApiBaseUrl = "http://192.168.0.100:5130";
	public const string ApiBaseUrlPreferenceKey = "ApiBaseUrl";

	public static HttpMessageHandler CreateHandler()
	{
		var handler = new HttpClientHandler();
#if DEBUG
		handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
#endif
		return handler;
	}

	private static string GetApiBaseUrl()
	{
		var configured = Environment.GetEnvironmentVariable("InventoryApiBaseUrl");
		if (!string.IsNullOrWhiteSpace(configured))
		{
			return configured.Trim();
		}

		var stored = Preferences.Default.Get(ApiBaseUrlPreferenceKey, string.Empty);
		if (!string.IsNullOrWhiteSpace(stored))
		{
			return stored.Trim();
		}

		if (DeviceInfo.Platform == DevicePlatform.Android)
		{
			return DeviceInfo.DeviceType == DeviceType.Virtual
				? "http://10.0.2.2:5130"
				: LanApiBaseUrl;
		}

		return "http://localhost:5130";
	}
}
