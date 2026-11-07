namespace Chords.App.Services;

/// <summary>Plays recorded notes (see <see cref="SampleSet"/>), each at a rate that shifts its pitch and speed together.</summary>
public interface ISoundBank
{
    /// <summary>
    /// Starts loading these samples in the background so they play without delay, and frees the others.
    /// A sample played before it is loaded is loaded then.
    /// </summary>
    void Prepare(IReadOnlyList<string> samples);

    /// <param name="sample">Sound file name without extension (<see cref="SampleSet.Names"/>).</param>
    /// <param name="rate">Playback rate; 2.0 is one octave up.</param>
    /// <returns>Id of the sounding note for <see cref="Stop"/>; 0 when it cannot play.</returns>
    int Play(string sample, float rate, float volume = 1f);

    /// <summary>Silences a note started by <see cref="Play"/>, unless it has already ended.</summary>
    void Stop(int id);

    /// <summary>Silences every note still ringing (like a guitarist damping before the next chord).</summary>
    void StopAll();
}
