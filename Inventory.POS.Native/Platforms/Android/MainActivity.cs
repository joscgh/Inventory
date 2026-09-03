using Android.App;
using Android;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;
using AndroidX.Core.Content;

namespace Inventory.POS.Native;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
	private const int CameraPermissionRequestCode = 1001;

	protected override void OnCreate(Bundle? savedInstanceState)
	{
		base.OnCreate(savedInstanceState);

		if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.Camera) != Permission.Granted)
		{
			ActivityCompat.RequestPermissions(this, new[] { Manifest.Permission.Camera }, CameraPermissionRequestCode);
		}
	}
}
