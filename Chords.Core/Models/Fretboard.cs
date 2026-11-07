using System;
using System.Collections.Generic;
using System.Linq;
using Chords.Core.Extensions;

namespace Chords.Core.Models
{
    /// <summary>A place on the neck where one of the shown notes sounds.</summary>
    public class FretboardNote
    {
        public int String { get; }
        /// <summary>Fret counted from the nut; the string's own nut or the capo when it is <see cref="Fretboard.OpenFret"/>.</summary>
        public int Fret { get; }
        /// <summary>Index of the note among the notes shown (0 = root).</summary>
        public int Index { get; }
        /// <summary>MIDI pitch it sounds.</summary>
        public int Pitch { get; }

        public FretboardNote(int s, int fret, int index, int pitch)
        {
            String = s;
            Fret = fret;
            Index = index;
            Pitch = pitch;
        }
    }

    /// <summary>
    /// Where the notes of a scale or a chord lie across the whole neck of an instrument, for showing them with their intervals.
    /// </summary>
    public static class Fretboard
    {
        /// <summary>Half-steps of the degrees of the major scale, the yardstick of interval names.</summary>
        private static readonly int[] MajorSteps = { 0, 2, 4, 5, 7, 9, 11 };

        /// <summary>
        /// Every place on the neck from the capo (or the nut) up to <paramref name="frets"/> where one of the notes sounds.
        /// </summary>
        /// <param name="info">The instrument without a capo; its frets count from the nut.</param>
        /// <param name="notes">Notes to find, the root first; enharmonic spellings match (C# is found where Db sounds).</param>
        /// <param name="capo">Fret of the capo: the strings sound from it, the frets below it are not played.</param>
        public static FretboardNote[] Find(InstrumentInfo info, IReadOnlyList<Note> notes, int capo, int frets)
        {
            var normalized = notes.Select(Note.Normalize).ToArray();
            var result = new List<FretboardNote>();

            for (var s = 0; s < info.StringCount; s++)
            {
                for (var fret = OpenFret(info, s, capo); fret <= frets; fret++)
                {
                    var index = Array.IndexOf(normalized, Note.Normalize(info.NoteAt(s, fret)));

                    if (index >= 0)
                    {
                        result.Add(new FretboardNote(s, fret, index, info.Pitches[s] + info.Semitones(s, fret)));
                    }
                }
            }

            return result.ToArray();
        }

        /// <summary>Fret a string rings open from: the capo, or for a short string (banjo's 5th) its own nut if that is higher.</summary>
        public static int OpenFret(InstrumentInfo info, int s, int capo) => Math.Max(capo, info.FirstFrets[s]);

        /// <summary>
        /// Name of an interval by the degree of the major scale it alters: 1, b3, #4, 5, bb7, 9…
        /// (more usual on a fretboard than m3 or A4).
        /// </summary>
        public static string DegreeName(Interval interval)
        {
            var steps = interval.ToSteps();
            var alteration = interval.ToHalftones() - (MajorSteps[steps % 7] + 12 * (steps / 7));
            var sign = alteration < 0 ? new string('b', -alteration) : new string('#', alteration);

            return sign + (steps + 1);
        }
    }
}
