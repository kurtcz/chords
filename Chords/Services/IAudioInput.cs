namespace Chords.Services;

/// <summary>Mono microphone stream, as raw as the platform gives it (no gain control or noise suppression).</summary>
public interface IAudioInput
{
    /// <summary>
    /// Starts recording; <paramref name="onSamples"/> receives each block of samples (−1…1) on a background thread.
    /// The microphone permission must already be granted.
    /// </summary>
    /// <returns>The sample rate, or 0 when the microphone cannot be opened.</returns>
    int Start(Action<float[], int> onSamples);

    void Stop();
}
