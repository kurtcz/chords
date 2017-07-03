#if DEBUG
//﻿#define TESTING
#endif
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Chords.Core.Models
{
    public class GuitarChordLayout
    {
        public const int X = -1;       //this symbol denotes a silent string
        private static readonly Note[] _strings =
        {
            new Note(Tone.E),
            new Note(Tone.A),
            new Note(Tone.D),
            new Note(Tone.G),
            new Note(Tone.B),
            new Note(Tone.E)
        };
        private static readonly Note[][] _stringNotes =
            Enumerable.Range(0, 6)
                      .Select(s => Enumerable.Range(0, 15)
                                             .Select(i => _strings[s].NoteAtChromaticDistance(i))
                                             .ToArray())
                      .ToArray();

		public Chord Chord { get; private set; }
		public int[] Positions { get; private set; } = new int[6];
        public GuitarChordType GuitarChordType { get; private set; }
        [JsonIgnore]
        public int Fret { get; private set; }
        [JsonIgnore]
        public Note[] Notes { get; private set; }
        [JsonIgnore]
        public bool Complete { get; private set; }

        public GuitarChordLayout(Chord chord, int[] positions)
        {
            Chord = chord;
            Positions = positions;

			Notes = GetUsedNotes();
			Complete = Notes.Distinct().Count() == Chord.Notes.Count();
			Fret = Positions.Any(i => i > 0)
					? Positions.Where(i => i > 0).Min()
					: 0;
			//if possible try to render chords from the 1st fret
			if (Positions.All(i => i <= 4))
			{
				Fret = 1;
			}
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
#if TESTING
            var k = 0;
            layouts = layouts.Skip(k).ToArray();
#endif
            var guitarChordTypes = Enum.GetValues(typeof(GuitarChordType))
                                       .Cast<GuitarChordType>()
                                       .Where(i => allowSpecial || i != GuitarChordType.Special)
                                       .ToList();
#if !TESTING
            Parallel.ForEach(layouts, layout =>
#else
            foreach(var layout in layouts)
#endif
            {
                foreach (var guitarChordType in guitarChordTypes)
                {
                    var result = new GuitarChordLayout(chord, layout)
                    {
                        GuitarChordType = guitarChordType
                    };
                    var b = result.IsValid(allowPartial);
                    if (b)
                    {
                        results.Add(result);
                    }
                }
			}
#if !TESTING
            );
#endif

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
                    if (s == 0 && positions[0] == -1 && positions[1] == 3 && positions[2] == 2 && positions[3] == 3 && positions[4] == 1 && positions[5] == 0)
                    {
                        s = s + 1 - 1;
                    }
                    //TODO: implement 1st 3 checks from IsValid() here
                    if ((pos == X && (s == 0 || positions[s + 1] != X)) ||
                        (pos != X && (positions[s + 1] == X || _stringNotes[s+1][positions[s + 1]] != _stringNotes[s][positions[s]])))
                    {
                        yield return positions;
                    }
                }
            }
		}

        private Note[] GetUsedNotes()
        {
            var notesUsed = new List<Note>(6);

			for (var s = 0; s < 6; s++)
			{
				if (Positions[s] < 0)
				{
					continue;
				}
                var note = _stringNotes[s][Positions[s]];
                var noteIndex = Array.IndexOf(Chord.NormalizedNotes, note);

                if (noteIndex >= 0)
				{
					notesUsed.Add(Chord.Notes[noteIndex]);
				}
			}

            return notesUsed.ToArray();
		}

        private bool IsValid(bool allowPartial)
        {
            //have we managed to find all the notes from the given chord?
            //if not we may leave out one note
            if ((!allowPartial && 
                 Chord.Notes.Any(i => !Notes.Contains(i))) ||
                Chord.Notes.Any(i => !Notes.Contains(i) &&
                                     !Chord.NonMandatoryNotes.Contains(i)) ||
				Chord.Notes.Count(i => !Notes.Contains(i) &&
									   Chord.NonMandatoryNotes.Contains(i)) > 1)
			{
				return false;
			}
            //do not allow same notes to follow each other immediately
            if (Notes.Select((i, idx) => new { Note = i, Index = idx })
                         .Any(i => i.Index < Notes.Length - 1 &&
                                   i.Note.Tone == Notes[i.Index + 1].Tone))
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
                 (Positions[0] != X &&
                  Positions.Any(i => i == X))))
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
