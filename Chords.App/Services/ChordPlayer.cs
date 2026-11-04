using Chords.Core.Models;

namespace Chords.App.Services;

public class ChordPlayer(ISoundBank soundBank)
{
    /// <summary>Strums a chord diagram twice (slow, then fast). -1 = muted string.</summary>
    public void PlayLayout(int[] positions, float volume = 1f) => Task.Run(async () =>
    {
        for (var pass = 0; pass < 2; pass++)
        {
            for (var i = 0; i < positions.Length; i++)
            {
                if (positions[i] < 0)
                {
                    continue;
                }
                soundBank.PlayString(i, Note.HalfStepsToPlayRate(positions[i]), volume);
                await Task.Delay(pass == 0 ? 200 : 50);
            }
            await Task.Delay(200);
        }
    });

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
