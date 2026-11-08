namespace Chords.Services;

/// <summary>
/// Widths (CSS pixels) of the system bars and display cutouts at the left and right edges, which the web view
/// extends under on Android (in landscape the navigation bar and the camera cutout sit at the sides). The page
/// pads its content by them (--side-inset-left/right in wwwroot/css/style.css), as with env(safe-area-inset-*)
/// on iOS: Android WebView before version 140 (all that Android 7–9 get) reports those as 0.
/// </summary>
public static class SideInsets
{
    public static double Left { get; private set; }
    public static double Right { get; private set; }

    /// <summary>Whether the platform reports the side insets here; otherwise the page uses env().</summary>
    public static bool Known { get; private set; }

    public static event Action Changed;

    public static void Set(double left, double right)
    {
        if (Known && left == Left && right == Right)
            return;
        (Left, Right, Known) = (left, right, true);
        Changed?.Invoke();
    }
}
