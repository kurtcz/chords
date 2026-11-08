using Chords.Services;

namespace Chords;

public class App : Application
{
    private readonly AppState state;

    public App(AppState state)
    {
        this.state = state;
        state.Attach(this);
    }

    protected override Window CreateWindow(IActivationState activationState)
        => new(new MainPage(state)) { Title = "Chord Expert" };
}
