using Android.App;
using Android.Content.PM;

namespace Chords.App;

// Keep the old activity name: launcher shortcuts of users updating from the
// Xamarin version point at it.
[Activity(
    Name = "com.tnemec.chords.android.MainActivity",
    Label = "ChordGuide",
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    Exported = true,
    LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Android.OS.Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        // Volume keys control the chord playback volume.
        VolumeControlStream = Android.Media.Stream.Music;
    }
}
