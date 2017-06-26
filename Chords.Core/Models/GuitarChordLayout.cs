﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chords.Core.Models
{
    public class GuitarChordLayout
    {
        const int X = -1;       //this symbol denotes a silent string
        private Chord _chord;
        private static readonly Note[] _strings = 
		{
            new Note(Tone.E),
			new Note(Tone.A),
			new Note(Tone.D),
			new Note(Tone.G),
			new Note(Tone.B),
			new Note(Tone.E)
		};

        public int[] Positions { get; private set; } = new int[6];
        public GuitarChordType GuitarChordType { get; private set; }
        public int Fret { get; private set; }
        public Note[] Notes { get; private set; }
        public bool Complete { get; private set; }

        private GuitarChordLayout(Chord chord)
        {
            _chord = chord;
        }

        public static IEnumerable<GuitarChordLayout> Generate(Chord chord, bool allowSpecial, bool allowPartial)
        {
            var results = new ConcurrentBag<GuitarChordLayout>();
            var positionsPerString = new HashSet<int>[6];

            //for each string find all positions corresponding to notes from the given chord
            for (var s = 0; s < 6; s++)
            {
                positionsPerString[s] = new HashSet<int>();
                positionsPerString[s].Add(X);

                if (chord.Notes.Any(i => i == _strings[s]))
                {
                    positionsPerString[s].Add(0);
                }
                for (var pos = 1; pos <= 10; pos++)
                {
                    var note = _strings[s].NoteAtChromaticDistance(pos);

                    if (chord.Notes.Any(i => Note.Normalize(i) == note))
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
            Parallel.ForEach(layouts, layout =>
            //foreach(var layout in layouts)
            {
                foreach (var guitarChordType in guitarChordTypes)
                {
                    var result = new GuitarChordLayout(chord)
                    {
                        GuitarChordType = guitarChordType,
                        Positions = layout
                    };

                    if (result.IsValid(allowPartial))
                    {
                        result.Notes = result.GetUsedNotes();
                        result.Complete = result.Notes.Distinct().Count() == result._chord.Notes.Count();
                        result.Fret = result.Positions.Where(i => i > 0).Min();
                        //if possible try to render normal chords from the 1st fret
                        if (guitarChordType != GuitarChordType.SixStringBarre &&
                            guitarChordType != GuitarChordType.FiveStringBarre &&
                            result.Fret <= 4 &&
                            result.Positions.All(i => i <= 4))
                        {
                            result.Fret = 1;
                        }
                        results.Add(result);
                    }
                }
            });
            //}

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

        private Note[] GetUsedNotes()
        {
			var notesUsed = new List<Note>();

			for (var s = 0; s < 6; s++)
			{
				if (Positions[s] < 0)
				{
					continue;
				}
                var note = _strings[s].NoteAtChromaticDistance(Positions[s]);
                var noteFromChord = _chord.Notes.FirstOrDefault(i => Note.Normalize(i) == note);

                if (noteFromChord != null)
				{
					notesUsed.Add(noteFromChord);
				}
			}

            return notesUsed.ToArray();
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
            var usedPositions = Positions.Where(i => i > 0);
            var maxUsedPosition = usedPositions.Any() ? usedPositions.Max() : 0;
            var minUsedPosition = usedPositions.Any() ? usedPositions.Min() : 0;
            if (maxUsedPosition - minUsedPosition + 1 > 4)
            {
                return false;
            }
			//for normal chords it is not possible to hold more than 4 notes
			if (GuitarChordType != GuitarChordType.SixStringBarre &&
				GuitarChordType != GuitarChordType.FiveStringBarre &&
				Positions.Count(i => i != X && i != 0) > 4)
			{
				return false;
			}
			//for special chords it is not possible to hold more than 4 notes
            //unless we hold them in a barre position in which case we have up to 3 fingers remaining
			if (GuitarChordType == GuitarChordType.Special &&
				Positions.Count(i => i > 0) > 4 &&
                ((Positions[0] > X &&
                  Positions.Count(i => i != Positions[0] &&
                                       i > 0) > 3) ||
				 (Positions[0] == X &&
                  Positions[1] > X &&
                  Positions.Count(i => i != Positions[1] &&
                                       i > 0) > 3)))
			{
				return false;
			}
			//special chords (with silent strings in the middle) shall not contain more than two silent strings
			if (GuitarChordType == GuitarChordType.Special &&
				Positions.Count(i => i == X) > 2)
			{
				return false;
			}
			//normal chords may not have a silent string in the middle
			if (GuitarChordType == GuitarChordType.SixString &&
				Positions.Any(i => i == X))
			{
				return false;
			}
			//normal chords may not have a silent string in the middle
			if (GuitarChordType == GuitarChordType.FiveString &&
				((Positions[0] == X &&
				  Positions.Count(i => i == X) > 1) ||
				 Positions.Any(i => i == X)))
			{
				return false;
			}
			//normal chords may not have a silent string in the middle
			if (GuitarChordType == GuitarChordType.FourString &&
				((Positions[0] == X &&
				  Positions[1] == X &&
				  Positions.Count(i => i == X) > 2) ||
				 (Positions[0] == X &&
				  Positions[1] == 0 &&
				  Positions.Count(i => i == X) > 1) ||
				 (Positions[0] != X &&
                  Positions.Any(i => i == X))))
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
                //exception for special type chords held in a barre way
                if (GuitarChordType != GuitarChordType.Special ||
                    (Positions[0] > 0 &&
                     Positions.Count(i => i > Positions[0]) > 3) ||
					(Positions[0] == X &&
                     Positions[1] > 0 &&
                     Positions.Count(i => i > Positions[1]) > 3) ||
                    (Positions.Where(i => i > 0).Distinct().Count() > 3))

				{
                    return false;
                }
            }
			//chords that do not span all strings may not use low strings
			if ((GuitarChordType == GuitarChordType.FiveString ||
				 GuitarChordType == GuitarChordType.FiveStringBarre) &&
				(Positions[0] != X &&
				 Positions[0] != 0))
			{
				return false;
			}
			//chords that do not span all strings may not use low strings
			if (GuitarChordType == GuitarChordType.FourString &&
				((Positions[0] != X &&
				  Positions[0] != 0) ||
				 (Positions[1] != X &&
				  Positions[1] != 0)))
			{
				return false;
			}
			//big barre may not have any silent or empty strings
			if (GuitarChordType == GuitarChordType.SixStringBarre &&
				Positions.Any(i => i == X || i == 0))
			{
				return false;
			}
			//small barre needs to have at least 5 strings that are not silent
			if (GuitarChordType == GuitarChordType.FiveStringBarre &&
				Positions.Count(i => i == X) > 1)
			{
				return false;
			}
            //small barre may not have any empty or silent strings other than the lowest one
			if (GuitarChordType == GuitarChordType.FiveStringBarre &&
				Positions[0] == 0 &&
				(Positions.Count(i => i == 0) > 1 ||
                 Positions.Any(i => i == X)))
			{
				return false;
			}
			//big barre has to be played using an index finger
			//therefore all other positions need to be bigger than the position on the lowest string
			if (GuitarChordType == GuitarChordType.SixStringBarre &&
				Positions.Any(i => i < Positions[0]))
			{
				return false;
			}
			//big barre has to be played using an index finger
			//therefore all other positions need to be bigger than the position on the 2nd lowest string
			//(optional "X" on the lowest string is allowed)
			if (GuitarChordType == GuitarChordType.FiveStringBarre &&
                (Positions[1] <= 0 ||
				 (Positions.Any(i => i < Positions[1] && i != X))))
			{
				return false;
			}
            //all barre chords may have up to 3 notes played by fingers other than index finger
            if (GuitarChordType == GuitarChordType.SixStringBarre &&
                Positions.Count(i => i != Positions[0] && i > 0) > 3)
            {
                return false;
            }
			//all barre chords may have up to 3 notes played by fingers other than index finger
			if (GuitarChordType == GuitarChordType.FiveStringBarre &&
				Positions.Count(i => i != Positions[1] && i > 0) > 3)
			{
				return false;
			}
			//distance between all barre notes may span more than 3 frets
			//(pinky is an exception, but it must be the only one)
			if ((GuitarChordType == GuitarChordType.SixStringBarre ||
				 GuitarChordType == GuitarChordType.FiveStringBarre) &&
                maxUsedPosition - minUsedPosition + 1 > 3)
			{
                var nonBareStrings = Positions.Select((i, idx) => new { pos = i, idx = idx })
                                                 .Where(i => i.pos > usedPositions.Min())   //min corresponds to barre position
                                                 .Select(i => i.idx);
                if (Positions[nonBareStrings.Last()] != maxUsedPosition ||
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
            unchecked
            {
                var result = 0;

                foreach(var position in Positions)
                {
					result = (result * 397) ^ position.GetHashCode();
				}

                return result;
            }
        }

		public override string ToString()
        {
            return string.Join(" ", Positions.Select(i => i < 0 ? "X" : i.ToString()));
        }
    }
}
