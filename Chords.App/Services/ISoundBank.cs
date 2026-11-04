namespace Chords.App.Services;

/// <summary>Plays the six recorded open guitar strings (0 = low E ... 5 = high E).</summary>
public interface ISoundBank
{
    /// <param name="stringIndex">0 = E6, 1 = A5, 2 = D4, 3 = G3, 4 = B2, 5 = E1.</param>
    /// <param name="rate">Playback rate; 2.0 is one octave up.</param>
    void PlayString(int stringIndex, float rate, float volume = 1f);

    /// <summary>Silences every string still ringing (like a guitarist damping before the next chord).</summary>
    void StopAll();
}
