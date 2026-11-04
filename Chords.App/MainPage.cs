using Chords.App.Components;
using Microsoft.AspNetCore.Components.WebView.Maui;

namespace Chords.App;

public class MainPage : ContentPage
{
    // Matches the fixed Bootstrap navbar, so the status bar area blends in.
    public static readonly Color NavbarColor = Color.FromArgb("#222222");

    // Matches the tab bar (wwwroot/css/style.css), so the home indicator area blends in.
    public static readonly Color TabBarColor = Color.FromArgb("#f8f8f8");

    public MainPage()
    {
        BackgroundColor = NavbarColor;
        // Only the safe areas show the page background: the navbar colour above, the tab bar colour below.
        // The split lies low, so the screen stays mostly dark while the web view starts.
        Background = new LinearGradientBrush(
            [new GradientStop(NavbarColor, 0.85f), new GradientStop(TabBarColor, 0.85f)],
            new Point(0, 0), new Point(0, 1));
        // Keep the web content out of the status bar, navigation bar and display cutouts.
        SafeAreaEdges = SafeAreaEdges.All;

        var webView = new BlazorWebView { HostPage = "wwwroot/index.html" };
        webView.RootComponents.Add(new RootComponent { Selector = "#app", ComponentType = typeof(Routes) });
#if IOS
        // iOS has no back button: allow the standard edge swipe to go back.
        webView.BlazorWebViewInitialized += (_, e) => e.WebView.AllowsBackForwardNavigationGestures = true;
#endif
        Content = webView;
    }
}
