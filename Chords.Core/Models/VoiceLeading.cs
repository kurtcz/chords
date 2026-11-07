using System;
using System.Collections.Generic;
using System.Linq;

namespace Chords.Core.Models
{
    /// <summary>
    /// Chooses the fingerings of a chord progression so that the hand moves as little as possible from chord to chord
    /// (smooth transitions, the guitarist's voice leading), while each fingering stays easy to hold.
    /// </summary>
    /// <remarks>
    /// The cost of a choice is the sum of the fingerings' own costs (<see cref="ShapeCost"/>) and of the moves
    /// between neighbouring chords (<see cref="MoveCost"/>), the last chord moving back to the first because
    /// progressions are played round and round. The cheapest choice is found by dynamic programming over the chords.
    /// </remarks>
    public static class VoiceLeading
    {
        /// <summary>Only the cheapest fingerings of each chord take part; the rest are rarely worth the move.</summary>
        private const int Candidates = 40;

        /// <summary>A fret the hand moves along the neck beyond <see cref="FreeJump"/>.</summary>
        private const double JumpPerFret = 1.5;
        /// <summary>Frets the hand moves without effort: the fingers reach them from where it is.</summary>
        private const double FreeJump = 2;
        /// <summary>A finger that stays on its string and fret from one chord to the next.</summary>
        private const double HeldFinger = 0.5;
        /// <summary>A string that changes what it plays (another fret, open, or silenced).</summary>
        private const double ChangedString = 0.3;
        /// <summary>A semitone the highest note (the one heard as the melody) moves.</summary>
        private const double TopVoicePerSemitone = 0.15;
        /// <summary>A fingering that leaves out a tone of the chord.</summary>
        private const double Incomplete = 3;
        /// <summary>A fingering whose bass is not the root.</summary>
        private const double NotRootBass = 3;
        /// <summary>A silent string of a strummed chord (a thin chord sounds weaker than the full one).</summary>
        private const double SilentString = 0.5;
        /// <summary>An open string ringing while the hand is far from the nut (an unusual sound and reach).</summary>
        private const double FarOpenString = 1;
        /// <summary>Hand position from which an open string counts as far from the nut.</summary>
        private const int FarFromNut = 5;
        /// <summary>A fret the hand sits up the neck beyond the first positions (beginners stay at the nut).</summary>
        private const double HighPerFret = 0.2;
        /// <summary>Hand position up to which the neck counts as the first positions.</summary>
        private const int FirstPositions = 4;
        /// <summary>A fingering the user marked as a favourite.</summary>
        private const double Preferred = -2;

        /// <summary>Index of the fingering chosen for each chord (0 for a chord without fingerings).</summary>
        /// <param name="options">Fingerings of each chord in the order shown to the user.</param>
        /// <param name="locked">Index the user picked for a chord, kept as it is; null entries are chosen.</param>
        /// <param name="preferred">Fingerings to prefer when the moves cost about the same (favourites).</param>
        public static int[] Choose(IReadOnlyList<IReadOnlyList<GuitarChordLayout>> options, IReadOnlyList<int?> locked = null,
                                   Func<GuitarChordLayout, bool> preferred = null)
        {
            var result = new int[options.Count];
            //chords without fingerings are skipped, their neighbours follow each other
            var chords = Enumerable.Range(0, options.Count).Where(i => options[i].Count > 0).ToArray();

            if (chords.Length == 0)
            {
                return result;
            }

            //candidate indices into options[chord] for each chord taking part
            var candidates = chords.Select(c =>
            {
                var fixedIndex = locked != null && c < locked.Count ? locked[c] : null;
                if (fixedIndex is int f && f >= 0 && f < options[c].Count)
                {
                    return new[] { f };
                }
                return Enumerable.Range(0, options[c].Count)
                                 .OrderBy(i => ShapeCost(options[c][i], preferred))
                                 .ThenBy(i => i)
                                 .Take(Candidates)
                                 .ToArray();
            }).ToArray();
            var shapes = candidates.Select((list, n) => list.Select(i => options[chords[n]][i]).ToArray()).ToArray();
            var shapeCosts = shapes.Select(list => list.Select(l => ShapeCost(l, preferred)).ToArray()).ToArray();
            var n = chords.Length;

            //moves between the candidates of neighbouring chords, the last one moving back to the first
            var moves = new double[n][,];
            for (var k = 0; k < n; k++)
            {
                var from = shapes[k];
                var to = shapes[(k + 1) % n];
                moves[k] = new double[from.Length, to.Length];
                for (var a = 0; a < from.Length; a++)
                {
                    for (var b = 0; b < to.Length; b++)
                    {
                        moves[k][a, b] = MoveCost(from[a], to[b]);
                    }
                }
            }

            var best = double.MaxValue;
            int[] bestPath = null;

            //the progression is a loop: try each fingering of the first chord and close the loop back to it
            for (var start = 0; start < shapes[0].Length; start++)
            {
                var cost = new double[shapes[0].Length];
                var back = new int[n][];
                for (var a = 0; a < cost.Length; a++)
                {
                    cost[a] = a == start ? shapeCosts[0][a] : double.MaxValue;
                }
                for (var k = 1; k < n; k++)
                {
                    var next = new double[shapes[k].Length];
                    back[k] = new int[next.Length];
                    for (var b = 0; b < next.Length; b++)
                    {
                        next[b] = double.MaxValue;
                        for (var a = 0; a < cost.Length; a++)
                        {
                            if (cost[a] == double.MaxValue)
                            {
                                continue;
                            }
                            var c = cost[a] + moves[k - 1][a, b];
                            if (c < next[b])
                            {
                                next[b] = c;
                                back[k][b] = a;
                            }
                        }
                        next[b] += shapeCosts[k][b];
                    }
                    cost = next;
                }

                for (var last = 0; last < cost.Length; last++)
                {
                    var total = cost[last] + (n > 1 ? moves[n - 1][last, start] : 0);
                    if (total < best - 1e-9)
                    {
                        best = total;
                        bestPath = new int[n];
                        bestPath[n - 1] = last;
                        for (var k = n - 1; k > 0; k--)
                        {
                            bestPath[k - 1] = back[k][bestPath[k]];
                        }
                    }
                }
            }

            for (var k = 0; k < n; k++)
            {
                result[chords[k]] = candidates[k][bestPath[k]];
            }
            return result;
        }

        /// <summary>What holding the fingering costs on its own: its difficulty, and a little for missing tones or another bass.</summary>
        public static double ShapeCost(GuitarChordLayout layout, Func<GuitarChordLayout, bool> preferred = null)
        {
            var cost = (double)layout.Difficulty;

            if (!layout.Complete)
            {
                cost += Incomplete;
            }
            if (layout.Notes.Length > 0 && Note.Normalize(layout.Notes[0]) != Note.Normalize(layout.Chord.Root))
            {
                cost += NotRootBass;
            }
            var info = layout.InstrumentInfo;
            if (!info.AllowInnerMutedStrings)
            {
                cost += SilentString * layout.Positions.Count(i => i == GuitarChordLayout.X);
            }
            var hand = HandPosition(layout);
            if (hand is double h)
            {
                if (h > FarFromNut)
                {
                    //the banjo's short drone string rings open in every position
                    cost += FarOpenString * layout.Positions.Where((pos, s) => pos == 0 && !info.IsShortString(s)).Count();
                }
                cost += HighPerFret * Math.Max(0, h - FirstPositions);
            }
            if (preferred != null && preferred(layout))
            {
                cost += Preferred;
            }
            return cost;
        }

        /// <summary>What moving from one fingering to the next costs: the hand's jump along the neck and the fingers that move.</summary>
        public static double MoveCost(GuitarChordLayout from, GuitarChordLayout to)
        {
            var cost = 0.0;
            var jump = Distance(from, to);

            cost += JumpPerFret * Math.Max(0, jump - FreeJump);

            var strings = Math.Min(from.Positions.Length, to.Positions.Length);
            for (var s = 0; s < strings; s++)
            {
                var a = from.Positions[s];
                var b = to.Positions[s];
                if (a == b)
                {
                    if (a > 0)
                    {
                        cost -= HeldFinger;
                    }
                }
                else
                {
                    cost += ChangedString;
                }
            }

            var top1 = TopPitch(from);
            var top2 = TopPitch(to);
            if (top1 != null && top2 != null)
            {
                cost += TopVoicePerSemitone * Math.Abs(top1.Value - top2.Value);
            }
            return cost;
        }

        /// <summary>
        /// Frets the hand moves between the fingerings: the distance of their hand positions,
        /// 0 when either needs no fingers (open strings only).
        /// </summary>
        public static double Distance(GuitarChordLayout from, GuitarChordLayout to)
        {
            var p1 = HandPosition(from);
            var p2 = HandPosition(to);

            return p1 == null || p2 == null ? 0 : Math.Abs(p1.Value - p2.Value);
        }

        /// <summary>Middle of the frets the fingering presses, or null when it presses none.</summary>
        public static double? HandPosition(GuitarChordLayout layout)
        {
            var used = layout.Positions.Where(i => i > 0).ToArray();

            return used.Length == 0 ? (double?)null : (used.Min() + used.Max()) / 2.0;
        }

        /// <summary>Pitch of the highest sounding string.</summary>
        private static int? TopPitch(GuitarChordLayout layout)
        {
            var info = layout.InstrumentInfo;
            int? top = null;

            for (var s = 0; s < layout.Positions.Length; s++)
            {
                var pos = layout.Positions[s];
                if (pos != GuitarChordLayout.X)
                {
                    var pitch = info.Pitches[s] + info.Semitones(s, pos);
                    if (top == null || pitch > top)
                    {
                        top = pitch;
                    }
                }
            }
            return top;
        }
    }
}
