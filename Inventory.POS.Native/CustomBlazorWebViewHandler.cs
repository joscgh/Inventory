#if ANDROID
using Android.Webkit;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Maui.Handlers;

namespace Inventory.Native
{
    public class CustomBlazorWebViewHandler : BlazorWebViewHandler
    {
        protected override void ConnectHandler(global::Android.Webkit.WebView platformView)
        {
            base.ConnectHandler(platformView);
            platformView.Settings.MediaPlaybackRequiresUserGesture = false;
            platformView.Settings.JavaScriptEnabled = true;
            platformView.SetWebChromeClient(new MauiBlazorWebChromeClient());
        }
    }
}
#endif