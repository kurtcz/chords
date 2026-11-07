using System;
using System.Linq;

namespace Chords.Core.Models
{
    /// <summary>
    /// Which finger of the fretting hand presses each string of a chord shape (1 = index … 4 = little finger).
    /// </summary>
    /// <remarks>
    /// Every way of giving the pressed strings fingers is tried and the cheapest kept. A way is possible when a higher finger
    /// never lies on a lower fret than a lower finger, and a finger on more strings (a barre) lies on one fret and does not
    /// cross an open string or a string pressed lower. The cost counts, in half points:
    /// <list type="bullet">
    /// <item>2 per fret a finger reaches past its place (finger n is at home n − 1 frets above the lowest pressed fret),</item>
    /// <item>1 per fret a finger stays below its place (fingers crowd on one fret easily),</item>
    /// <item>1 when on one fret a lower string gets a higher finger,</item>
    /// <item>5 for a barre of the index finger, 7 for a barre of another finger.</item>
    /// </list>
    /// So C is x 3 2 0 1 0 → 3 2 1, G 3 2 0 0 0 3 → 2 1 3, D x x 0 2 3 2 → 1 3 2 and F 1 3 3 2 1 1 → 1 3 4 2 1 1.
    /// </remarks>
    public static class Fingering
    {
        public const int Fingers = 4;

        private const int StretchCost = 2;
        private const int CrowdCost = 1;
        private const int CrossedCost = 1;
        private const int IndexBarreCost = 5;
        private const int BarreCost = 7;

        /// <summary>Finger of each string (0 for an open or muted string), or null when the shape cannot be held with four fingers.</summary>
        /// <param name="positions">Fret of each string from the lowest one, 0 open, -1 muted.</param>
        public static int[] Assign(int[] positions)
        {
            var pressed = Enumerable.Range(0, positions.Length).Where(s => positions[s] > 0).ToArray();
            var result = new int[positions.Length];

            if (pressed.Length == 0)
            {
                return result;
            }
            //a single finger by open strings: the hand stays in the first position, finger n on fret n (C 0 0 0 3 on the ukulele)
            if (pressed.Length == 1 && positions.Contains(0) && positions[pressed[0]] <= Fingers)
            {
                result[pressed[0]] = positions[pressed[0]];
                return result;
            }
            var lowest = pressed.Min(s => positions[s]);
            var fingers = new int[pressed.Length];
            int[] best = null;
            var bestCost = int.MaxValue;

            void Try(int i)
            {
                if (i == pressed.Length)
                {
                    var cost = Cost(positions, pressed, fingers, lowest);

                    if (cost < bestCost)
                    {
                        bestCost = cost;
                        best = fingers.ToArray();
                    }
                    return;
                }
                for (var finger = 1; finger <= Fingers; finger++)
                {
                    fingers[i] = finger;
                    Try(i + 1);
                }
            }

            Try(0);
            if (best == null)
            {
                return null;
            }
            for (var i = 0; i < pressed.Length; i++)
            {
                result[pressed[i]] = best[i];
            }

            return result;
        }

        /// <summary>Cost of holding the strings with these fingers, int.MaxValue when it cannot be done.</summary>
        private static int Cost(int[] positions, int[] pressed, int[] fingers, int lowest)
        {
            var cost = 0;

            for (var i = 0; i < pressed.Length; i++)
            {
                var fret = positions[pressed[i]];
                var reach = fret - (lowest + fingers[i] - 1);

                cost += reach > 0 ? reach * StretchCost : -reach * CrowdCost;

                for (var j = i + 1; j < pressed.Length; j++)
                {
                    var other = positions[pressed[j]];

                    if (fingers[i] == fingers[j])
                    {
                        if (fret != other)
                        {
                            return int.MaxValue;
                        }
                    }
                    //strings go up from i to j: a higher finger must not lie on a lower fret
                    else if (fingers[i] < fingers[j] ? fret > other : other > fret)
                    {
                        return int.MaxValue;
                    }
                    else if (fret == other && fingers[i] > fingers[j])
                    {
                        cost += CrossedCost;
                    }
                }
            }
            for (var finger = 1; finger <= Fingers; finger++)
            {
                var strings = Enumerable.Range(0, pressed.Length).Where(i => fingers[i] == finger).Select(i => pressed[i]).ToArray();

                if (strings.Length < 2)
                {
                    continue;
                }
                var fret = positions[strings[0]];

                for (var s = strings[0] + 1; s < strings[strings.Length - 1]; s++)
                {
                    if (positions[s] == 0 || (positions[s] > 0 && positions[s] < fret))
                    {
                        return int.MaxValue;
                    }
                }
                cost += finger == 1 ? IndexBarreCost : BarreCost;
            }

            return cost;
        }
    }
}
