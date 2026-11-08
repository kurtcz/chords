using Chords.Components;
using Chords.Services;
using Microsoft.AspNetCore.Components.WebView.Maui;

namespace Chords;

public class MainPage : ContentPage
{
    // Matches the fixed Bootstrap navbar, so the status bar area blends in; the same in both themes.
    public static readonly Color NavbarColor = Color.FromArgb("#222222");

    // Match the tab bar (--tab-bar-bg in wwwroot/css/style.css), so the home indicator area blends in.
    public static readonly Color TabBarColor = Color.FromArgb("#f8f8f8");
    public static readonly Color DarkTabBarColor = Color.FromArgb("#1c1c1e");

    private readonly AppState state;

    public MainPage(AppState state)
    {
        this.state = state;
        BackgroundColor = NavbarColor;
        UpdateColors();
        state.ThemeChanged += () => Dispatcher.Dispatch(UpdateColors);
        // The Android window may not be there yet when the page is created.
        Loaded += (_, _) => UpdateColors();
        var webView = new BlazorWebView { HostPage = "wwwroot/index.html" };
        webView.RootComponents.Add(new RootComponent { Selector = "#app", ComponentType = typeof(Routes) });
        // Keep the web content out of the status bar and the navigation bar / home indicator at the bottom: those
        // areas show the page background. At the sides (landscape) the web view goes under the bars and cutouts,
        // so the title bar, page and tab bar colours reach the screen edges; the page pads its content by
        // env(safe-area-inset-left/right) on iOS and by SideInsets on Android.
        SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.None, SafeAreaRegions.All, SafeAreaRegions.None, SafeAreaRegions.All);
#if ANDROID
        webView.BlazorWebViewInitialized += (_, e) =>
        {
            var view = e.WebView;
            void Update()
            {
                if (AndroidX.Core.View.ViewCompat.GetRootWindowInsets(view) is not { } insets)
                    return;
                var sides = insets.GetInsets(AndroidX.Core.View.WindowInsetsCompat.Type.SystemBars()
                                             | AndroidX.Core.View.WindowInsetsCompat.Type.DisplayCutout());
                var density = view.Resources!.DisplayMetrics!.Density;
                SideInsets.Set(sides.Left / density, sides.Right / density);
            }
            // Rotating changes the insets and lays the web view out again.
            view.LayoutChange += (_, _) => Update();
            Update();
        };
#endif
#if IOS
        // iOS has no back button: allow the standard edge swipe to go back.
        webView.BlazorWebViewInitialized += (_, e) => e.WebView.AllowsBackForwardNavigationGestures = true;
#endif
        Content = webView;
    }

    /// <summary>Colours of the safe areas and system bars for the current theme.</summary>
    private void UpdateColors()
    {
        var tabBar = state.IsDark ? DarkTabBarColor : TabBarColor;
        // Only the safe areas show the page background: the navbar colour above, the tab bar colour below.
        // The split lies low, so the screen stays mostly dark while the web view starts.
        Background = new LinearGradientBrush(
            [new GradientStop(NavbarColor, 0.85f), new GradientStop(tabBar, 0.85f)],
            new Point(0, 0), new Point(0, 1));
#if ANDROID
        // Icons of the status bar stay light on the dark navbar; those of the navigation bar follow the tab bar.
        if (Platform.CurrentActivity?.Window is { } window)
        {
            var insets = AndroidX.Core.View.WindowCompat.GetInsetsController(window, window.DecorView);
            insets.AppearanceLightStatusBars = false;
            insets.AppearanceLightNavigationBars = !state.IsDark;
        }
#endif
    }
}
