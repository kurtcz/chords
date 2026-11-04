using Chords.Core.Models;

namespace Chords.App.Services;

public class ChordPlayer(ISoundBank soundBank)
{
    /// <summary>MIDI pitches of the recorded open guitar strings (sound bank indexes 0..5).</summary>
    private static readonly int[] SamplePitches = InstrumentInfo.Guitar.Pitches;

    /// <summary>Strums a chord diagram twice (slow, then fast). -1 = muted string.</summary>
    public void PlayLayout(InstrumentInfo instrument, int[] positions, float volume = 1f) => Task.Run(async () =>
    {
        for (var pass = 0; pass < 2; pass++)
        {
            for (var i = 0; i < positions.Length; i++)
            {
                if (positions[i] < 0)
                {
                    continue;
                }
                var (sample, rate) = SampleFor(instrument.Pitches[i], positions[i]);

                soundBank.PlayString(sample, rate, volume);
                await Task.Delay(pass == 0 ? 200 : 50);
            }
            await Task.Delay(200);
        }
    });

    /// <summary>
    /// Picks the highest recorded string not above the open string (on the guitar its own recording)
    /// and the rate that shifts it to the played note.
    /// Android's SoundPool only plays at 0.5–2×, so notes beyond that sound an octave closer.
    /// </summary>
    private static (int Sample, float Rate) SampleFor(int openPitch, int fret)
    {
        var sample = Math.Max(0, Array.FindLastIndex(SamplePitches, p => p <= openPitch));
        var rate = (float)Math.Pow(2, (openPitch + fret - SamplePitches[sample]) / 12.0);

        while (rate > 2f)
        {
            rate /= 2;
        }
        while (rate < 0.5f)
        {
            rate *= 2;
        }

        return (sample, rate);
    }

    /// <summary>Plays the notes of a chord (English name, e.g. "F#m") as a quick arpeggio on the A string.</summary>
    public void PlayChord(string id, float volume = 1f) => Task.Run(async () =>
    {
        if (!Chord.TryParse(id, NamingConvention.English, out var chord))
        {
            return;
        }
        var c = new Note(Tone.C);
        var minDistance = 3 + c.ChromaticDistance(chord.Notes[0]);

        foreach (var note in chord.Notes)
        {
            var pos = 3 + c.ChromaticDistance(note);

            if (pos < minDistance)
            {
                pos += 12;
            }
            soundBank.PlayString(1, Note.HalfStepsToPlayRate(pos), volume);
            await Task.Delay(50);
        }
    });
}
