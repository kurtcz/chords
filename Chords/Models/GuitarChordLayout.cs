﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chords.Models
{
    public class GuitarChordLayout
    {
        const int X = -1;       //this symbol denotes a silent string
        private Chord _chord;
        private static readonly Dictionary<NamingConvention, string[]> _strings = new Dictionary<NamingConvention, string[]>
		{
			{ NamingConvention.English, new[] { "E", "A", "D", "G", "B", "E" }},
			{ NamingConvention.German, new[] { "E", "A", "D", "G", "H", "E" }},
			{ NamingConvention.Latin, new[] { "Mi", "La", "Re", "Sol", "Si", "Mi" }}
		};
        public string[] Positions { get; private set; } = new string[6];
        public int[] IntPositions { get; private set; } = new int[6];
        public GuitarChordType GuitarChordType { get; private set; }
        public int Fret { get; private set; }
        public string[] Notes { get; private set; }
        public bool Complete { get; private set; }

        private GuitarChordLayout(Chord chord)
        {
            _chord = chord;
        }

        public static IEnumerable<GuitarChordLayout> Generate(Chord chord, bool allowSpecial, bool allowPartial)
        {
			//var results = new HashSet<GuitarChordLayout>();
			var results = new ConcurrentBag<GuitarChordLayout>();
            var positionsPerString = new HashSet<int>[6];

            //for each string find all positions corresponding to notes from the given chord
            for (var s = 0; s < 6; s++)
            {
                positionsPerString[s] = new HashSet<int>();
                positionsPerString[s].Add(X);

				var freeNoteIndex1 = Array.IndexOf(Definitions.Notes[chord.NamingConvention], _strings[chord.NamingConvention][s]);
				var freeNoteIndex2 = Array.IndexOf(Definitions.Notes[chord.NamingConvention], _strings[chord.NamingConvention][s]);
				var freeNote1 = Definitions.Notes[chord.NamingConvention][freeNoteIndex1];
				var freeNote2 = Definitions.Notes[chord.NamingConvention][freeNoteIndex2];

				if (chord.Notes.Any(i => string.Equals(i, freeNote1, StringComparison.CurrentCultureIgnoreCase)))
                {
                    positionsPerString[s].Add(0);
                }
                for (var pos = 1; pos <= 10; pos++)
                {
					var noteIndex1 = (freeNoteIndex1 + pos) % Definitions.Notes[chord.NamingConvention].Length;
					var noteIndex2 = (freeNoteIndex2 + pos) % Definitions.Notes2[chord.NamingConvention].Length;
					var note1 = Definitions.Notes[chord.NamingConvention][noteIndex1];
					var note2 = Definitions.Notes2[chord.NamingConvention][noteIndex2];

					if (chord.Notes.Any(i => string.Equals(i, note1, StringComparison.CurrentCultureIgnoreCase)) ||
                        chord.Notes.Any(i => string.Equals(i, note2, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        positionsPerString[s].Add(pos);
                    }
				}
			}

            var layouts = GenerateLayouts(positionsPerString).ToArray();
            var guitarChordTypes = Enum.GetValues(typeof(GuitarChordType))
                                       .Cast<GuitarChordType>()
                                       .Where(i => allowSpecial || i != GuitarChordType.Special)
                                       .ToList();
            //foreach(var layout in layouts)
            Parallel.ForEach(layouts, layout =>
            {
                foreach (var guitarChordType in guitarChordTypes)
                {
                    var result = new GuitarChordLayout(chord)
                    {
                        GuitarChordType = guitarChordType,
                        IntPositions = layout
                    };

                    if (result.IsValid(allowPartial))
                    {
                        result.Positions = layout.Select(i => i < 0 ? "X" : i.ToString())
                                                 .ToArray();
                        result.Notes = result.GetUsedNotes().ToArray();
                        result.Complete = result.Notes.Distinct().Count() == result._chord.Notes.Count();
                        result.Fret = result.IntPositions.Where(i => i > 0).Min();
                        //if possible try to render normal chords from the 1st fret
                        if (guitarChordType != GuitarChordType.SixStringBarre &&
                            guitarChordType != GuitarChordType.FiveStringBarre &&
                            result.Fret <= 4 &&
                            result.IntPositions.All(i => i <= 4))
                        {
                            result.Fret = 1;
                        }
                        results.Add(result);
                    }
                }
            });

            return results.Distinct();
        }

        private static IEnumerable<int[]> GenerateLayouts(HashSet<int>[] positionsPerString)
        {
			return GetPositionsForStrings(positionsPerString, 0);
        }

        private static IEnumerable<int[]> GetPositionsForStrings(HashSet<int>[] positionsPerString, int s)
        {
            if (s == positionsPerString.Length - 1)
            {
                foreach (var pos in positionsPerString[s])
                {
					var positions = Enumerable.Repeat(X, 6).ToArray();

                    positions[s] = pos;

                    yield return positions;
                }
                yield break;
            }
            foreach (var pos in positionsPerString[s])
            {
                var positions0 = GetPositionsForStrings(positionsPerString, s + 1);

                foreach (var pos0 in positions0)
                {
                    var positions = new int[6];

                    pos0.CopyTo(positions, 0);
                    positions[s] = pos;

                    yield return positions;
                }
            }
		}

        private List<string> GetUsedNotes()
        {
			var notesUsed = new List<string>();

			for (var s = 0; s < 6; s++)
			{
				if (IntPositions[s] < 0)
				{
					continue;
				}
				var freeNoteIndex1 = Array.IndexOf(Definitions.Notes[_chord.NamingConvention], _strings[_chord.NamingConvention][s]);
				var freeNoteIndex2 = Array.IndexOf(Definitions.Notes[_chord.NamingConvention], _strings[_chord.NamingConvention][s]);
				var noteIndex1 = (freeNoteIndex1 + IntPositions[s]) % Definitions.Notes[_chord.NamingConvention].Length;
				var noteIndex2 = (freeNoteIndex2 + IntPositions[s]) % Definitions.Notes2[_chord.NamingConvention].Length;
				var note1 = Definitions.Notes[_chord.NamingConvention][noteIndex1];
				var note2 = Definitions.Notes2[_chord.NamingConvention][noteIndex2];

                if (_chord.Notes.Any(i => i == note1))
				{
					notesUsed.Add(note1);
				}
                else if (_chord.Notes.Any(i => i == note2))
				{
					notesUsed.Add(note2);
				}
			}

            return notesUsed;
		}

        private bool IsValid(bool allowPartial)
        {
            //have we managed to find all the notes from the given chord?
            //if not we may leave out one note
            var usedNotes = GetUsedNotes();
            if ((!allowPartial && 
                 _chord.Notes.Any(i => !usedNotes.Contains(i))) ||
                _chord.Notes.Any(i => !usedNotes.Contains(i) &&
                                      !_chord.NonMandatoryNotes.Contains(i)) ||
				_chord.Notes.Count(i => !usedNotes.Contains(i) &&
									    _chord.NonMandatoryNotes.Contains(i)) > 1)
			{
				return false;
			}
            //distance between positions should not exceed 4 frets
            var usedPositions = IntPositions.Where(i => i > 0);
            var maxUsedPosition = usedPositions.Any() ? usedPositions.Max() : 0;
            var minUsedPosition = usedPositions.Any() ? usedPositions.Min() : 0;
            if (maxUsedPosition - minUsedPosition + 1 > 4)
            {
                return false;
            }
			//for normal chords it is not possible to hold more than 4 notes
			if (GuitarChordType != GuitarChordType.SixStringBarre &&
				GuitarChordType != GuitarChordType.FiveStringBarre &&
				IntPositions.Count(i => i != X && i != 0) > 4)
			{
				return false;
			}
			////for special chords it is not possible to hold more than 4 notes
			if (GuitarChordType == GuitarChordType.Special &&
				IntPositions.Count(i => i > 0) > 4)
			{
				return false;
			}
			//special chords (with silent strings in the middle) shall not contain any empty strings
			if (GuitarChordType == GuitarChordType.Special &&
				IntPositions.Any(i => i == 0))
			{
				return false;
			}
			//normal chords may not have a silent string in the middle
			if (GuitarChordType == GuitarChordType.SixString &&
				IntPositions.Any(i => i == X))
			{
				return false;
			}
			//normal chords may not have a silent string in the middle
			if (GuitarChordType == GuitarChordType.FiveString &&
				((IntPositions[0] == X &&
				  IntPositions.Count(i => i == X) > 1) ||
				 IntPositions.Any(i => i == X)))
			{
				return false;
			}
			//normal chords may not have a silent string in the middle
			if (GuitarChordType == GuitarChordType.FourString &&
				((IntPositions[0] == X &&
				  IntPositions[1] == X &&
				  IntPositions.Count(i => i == X) > 2) ||
				 (IntPositions[0] == X &&
				  IntPositions[1] == 0 &&
				  IntPositions.Count(i => i == X) > 1) ||
				 (IntPositions[0] != X &&
                  IntPositions.Any(i => i == X))))
			{
				return false;
			}
			//non-barre chords may not have any holes so that they can be reached by most players
			var emptyPositions = Enumerable.Range(minUsedPosition, maxUsedPosition - minUsedPosition)
										   .Where(i => !usedPositions.Contains(i));
			if ((GuitarChordType == GuitarChordType.SixString ||
				 GuitarChordType == GuitarChordType.FiveString ||
                 GuitarChordType == GuitarChordType.FourString ||
                 GuitarChordType == GuitarChordType.Special) &&
                emptyPositions.Any())
            {
				return false;
            }
			//chords that do not span all strings may not use low strings
			if ((GuitarChordType == GuitarChordType.FiveString ||
				 GuitarChordType == GuitarChordType.FiveStringBarre) &&
				(IntPositions[0] != X &&
				 IntPositions[0] != 0))
			{
				return false;
			}
			//chords that do not span all strings may not use low strings
			if (GuitarChordType == GuitarChordType.FourString &&
				((IntPositions[0] != X &&
				  IntPositions[0] != 0) ||
				 (IntPositions[1] != X &&
				  IntPositions[1] != 0)))
			{
				return false;
			}
			//big barre may not have any silent or empty strings
			if (GuitarChordType == GuitarChordType.SixStringBarre &&
				IntPositions.Any(i => i == X || i == 0))
			{
				return false;
			}
			//small barre needs to have at least 5 strings that are not silent
			if (GuitarChordType == GuitarChordType.FiveStringBarre &&
				IntPositions.Count(i => i == X) > 1)
			{
				return false;
			}
            //small barre may not have any empty or silent strings other than the lowest one
			if (GuitarChordType == GuitarChordType.FiveStringBarre &&
				IntPositions[0] == 0 &&
				(IntPositions.Count(i => i == 0) > 1 ||
                 IntPositions.Any(i => i == X)))
			{
				return false;
			}
			//big barre has to be played using an index finger
			//therefore all other positions need to be bigger than the position on the lowest string
			if (GuitarChordType == GuitarChordType.SixStringBarre &&
				IntPositions.Any(i => i < IntPositions[0]))
			{
				return false;
			}
			//big barre has to be played using an index finger
			//therefore all other positions need to be bigger than the position on the 2nd lowest string
			//(optional "X" on the lowest string is allowed)
			if (GuitarChordType == GuitarChordType.FiveStringBarre &&
                (IntPositions[1] <= 0 ||
				 (IntPositions.Any(i => i < IntPositions[1] && i != X))))
			{
				return false;
			}
            //all barre chords may have up to 3 notes played by fingers other than index finger
            if (GuitarChordType == GuitarChordType.SixStringBarre &&
                IntPositions.Count(i => i != IntPositions[0] && i > 0) > 3)
            {
                return false;
            }
			//all barre chords may have up to 3 notes played by fingers other than index finger
			if (GuitarChordType == GuitarChordType.FiveStringBarre &&
				IntPositions.Count(i => i != IntPositions[1] && i > 0) > 3)
			{
				return false;
			}
			//distance between all barre notes may span more than 3 frets
			//(pinky is an exception, but it must be the only one)
			if ((GuitarChordType == GuitarChordType.SixStringBarre ||
				 GuitarChordType == GuitarChordType.FiveStringBarre) &&
                maxUsedPosition - minUsedPosition + 1 > 3)
			{
                var nonBareStrings = IntPositions.Select((i, idx) => new { pos = i, idx = idx })
                                                 .Where(i => i.pos > usedPositions.Min())   //min corresponds to barre position
                                                 .Select(i => i.idx);
                if (IntPositions[nonBareStrings.Last()] != maxUsedPosition ||
                    usedPositions.Count(i => i == maxUsedPosition) > 1 ||
                    maxUsedPosition - minUsedPosition + 1 > 4)
                {
                    return false;
                }
			}
			return true;
        }

		public override bool Equals(object obj)
		{
            var other = obj as GuitarChordLayout;

            if (other == null)
            {
                return false;
            }

            return Positions.SequenceEqual(other.Positions);
		}

        public override int GetHashCode()
        {
            return string.Join("", Positions).GetHashCode();
        }

		public override string ToString()
        {
            if (Positions.All(i => i.Length < 2))
            {
                return string.Join("", Positions);
            }
            return string.Join(" ", Positions);
        }
    }
}
