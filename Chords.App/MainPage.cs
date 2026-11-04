using Chords.App.Components;
using Microsoft.AspNetCore.Components.WebView.Maui;

namespace Chords.App;

public class MainPage : ContentPage
{
    // Matches the fixed Bootstrap navbar, so the status bar area blends in.
    public static readonly Color NavbarColor = Color.FromArgb("#222222");

    public MainPage()
    {
        BackgroundColor = NavbarColor;
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
