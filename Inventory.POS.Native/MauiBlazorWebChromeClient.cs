#if ANDROID
using Android.Webkit;

namespace Inventory.Native
{
    public class MauiBlazorWebChromeClient : WebChromeClient
    {
        public override void OnPermissionRequest(PermissionRequest? request)
        {
            if (request == null)
            {
                return;
            }

            var resources = request.GetResources();
            if (resources != null)
            {
                foreach (var resource in resources)
                {
                    if (resource == PermissionRequest.ResourceVideoCapture)
                    {
                        request.Grant(resources);
                        return;
                    }
                }
            }

            base.OnPermissionRequest(request);
        }
    }
}
#endif