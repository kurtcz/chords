namespace Chords.App;

public class App : Application
{
    protected override Window CreateWindow(IActivationState activationState)
        => new(new MainPage()) { Title = "ChordGuide" };
}
