using Android.App;
using Android.Content.PM;

namespace Chords;

// Keep the old activity name: launcher shortcuts of users updating from the
// Xamarin version point at it.
[Activity(
    Name = "com.tnemec.chords.android.MainActivity",
    Label = "Chord Expert",
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    Exported = true,
    LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Android.OS.Bundle savedInstanceState)
    {
        // Draw behind the system bars on every Android version, as Android 15+ does anyway for target SDK 35+
        // (Google Play asks for this). MainPage keeps the web view inside the safe area and paints the bar areas;
        // the bars stay transparent, without the system's contrast scrim. Icon colours are set by MainPage.
        // Below Android 8 the navigation bar icons can only be light, so it gets the usual dark scrim there.
        var transparent = Android.Graphics.Color.Transparent.ToArgb();
        AndroidX.Activity.EdgeToEdge.Enable(this,
            AndroidX.Activity.SystemBarStyle.Dark(transparent),
            AndroidX.Activity.SystemBarStyle.Light(transparent, Android.Graphics.Color.Argb(0x80, 0x1b, 0x1b, 0x1b).ToArgb()));
        base.OnCreate(savedInstanceState);
        // Volume keys control the chord playback volume.
        VolumeControlStream = Android.Media.Stream.Music;
    }
}
