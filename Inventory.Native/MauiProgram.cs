using Inventory.SharedUI;
using Inventory.SharedUI.Services;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.Logging;

#if ANDROID
using Android.Webkit;
#endif

namespace Inventory.Native
{
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

            builder.Services.AddMauiBlazorWebView();

#if ANDROID
            builder.ConfigureMauiHandlers(handlers =>
            {
                handlers.AddHandler<BlazorWebView, CustomBlazorWebViewHandler>();
            });
#endif

            var apiBaseUrl = GetApiBaseUrl();

            builder.Services.AddScoped(sp => new HttpClient(CreateHandler())
            {
                BaseAddress = new Uri(apiBaseUrl.TrimEnd('/') + "/"),
                Timeout = TimeSpan.FromSeconds(20)
            });
            builder.Services.AddInventoryApiServices();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

        // IP del PC donde corre Inventory.API, vista desde el teléfono en la misma red WiFi.
        // Si la IP del PC cambia, actualiza este valor (o cámbialo desde la pantalla
        // /servidor, que no requiere recompilar) y vuelve a generar el APK.
        //
        // Se usa HTTP a propósito: la app nativa no tiene la restricción de contenido
        // mixto del navegador, y así no depende de que el teléfono confíe en el
        // certificado de desarrollo. El puerto HTTPS de la API es el 5131.
        public const string LanApiBaseUrl = "http://192.168.0.100:5130";

        public const string ApiBaseUrlPreferenceKey = "ApiBaseUrl";

        // En DEBUG se acepta el certificado de desarrollo (autofirmado) por si se
        // apunta la app al endpoint HTTPS de la API.
        public static HttpMessageHandler CreateHandler()
        {
            var handler = new HttpClientHandler();
#if DEBUG
            handler.ServerCertificateCustomValidationCallback =
                (_, _, _, _) => true;
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

            // Permite cambiar el servidor desde la app sin recompilar (pantalla /servidor).
            var stored = Preferences.Default.Get(ApiBaseUrlPreferenceKey, string.Empty);
            if (!string.IsNullOrWhiteSpace(stored))
            {
                return stored.Trim();
            }

            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                // 10.0.2.2 sólo funciona en el emulador; en un teléfono real hace falta la IP de la LAN.
                return DeviceInfo.DeviceType == DeviceType.Virtual
                    ? "http://10.0.2.2:5130"
                    : LanApiBaseUrl;
            }

            return "http://localhost:5130";
        }
    }
}
