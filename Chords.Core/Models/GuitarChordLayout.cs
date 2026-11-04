﻿﻿#if DEBUG
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
        private const int MaxPosition = 15 + 3;
        private static readonly ConcurrentDictionary<Instrument, Note[][]> _stringNotesCache = new ConcurrentDictionary<Instrument, Note[][]>();

		public Chord Chord { get; private set; }
		public int[] Positions { get; private set; } = new int[6];
        public GuitarChordType GuitarChordType { get; private set; }
        public Instrument Instrument { get; private set; }
        [JsonIgnore]
        public InstrumentInfo InstrumentInfo => InstrumentInfo.Get(Instrument);
        [JsonIgnore]
        public int Fret { get; private set; }
        [JsonIgnore]
        public Note[] Notes { get; private set; }
        [JsonIgnore]
        public bool Complete { get; private set; }

        /// <summary>Notes at positions 0..<see cref="MaxPosition"/> for each string of the instrument.</summary>
        private static Note[][] StringNotes(Instrument instrument)
        {
            return _stringNotesCache.GetOrAdd(instrument, i =>
            {
                var info = InstrumentInfo.Get(i);

                return Enumerable.Range(0, info.StringCount)
                                 .Select(s => Enumerable.Range(0, MaxPosition + 1)
                                                        .Select(pos => info.NoteAt(s, info.CanFret(s, pos) ? pos : 0))
                                                        .ToArray())
                                 .ToArray();
            });
        }

        public GuitarChordLayout(Chord chord, int[] positions, GuitarChordType? guitarChordType = null, Instrument instrument = Instrument.Guitar)
        {
            Chord = chord;
            Positions = positions;
            Instrument = instrument;
			Notes = GetUsedNotes();
            if (guitarChordType.HasValue)
            {
                GuitarChordType = guitarChordType.Value;
            }
            else if (instrument != Instrument.Guitar)
            {
                GuitarChordType = GuitarChordType.Standard;
            }
            else
            {
                foreach (var chordType in Enum.GetValues(typeof(GuitarChordType)).Cast<GuitarChordType>().Where(i => i != GuitarChordType.Standard))
                {
                    GuitarChordType = chordType;
                    if (IsValid(true))
                    {
                        break;
                    }
                }
            }
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

        public static IEnumerable<GuitarChordLayout> Generate(Chord chord, bool allowBarre, bool allowSpecial, bool allowPartial, int maxFret)
        {
            return Generate(chord, Instrument.Guitar, allowBarre, allowSpecial, allowPartial, maxFret);
        }

        public static IEnumerable<GuitarChordLayout> Generate(Chord chord, Instrument instrument, bool allowBarre, bool allowSpecial, bool allowPartial, int maxFret)
        {
            var info = InstrumentInfo.Get(instrument);
            var stringNotes = StringNotes(instrument);
            var results = new ConcurrentBag<GuitarChordLayout>();
            var positionsPerString = new HashSet<int>[info.StringCount];

            //for each string find all positions corresponding to notes from the given chord
            for (var s = 0; s < info.StringCount; s++)
            {
                positionsPerString[s] = new HashSet<int>();
                //a short string (banjo's drone) may be muted unless it can ring open
                if (info.AllowMutedStrings ||
                    (info.IsShortString(s) && !chord.Notes.Any(i => Note.Normalize(i) == stringNotes[s][0])))
                {
                    positionsPerString[s].Add(X);
                }
                for (var pos = 0; pos <= maxFret + 3 && pos <= MaxPosition; pos++)
                {
                    if (info.CanFret(s, pos) &&
                        chord.Notes.Any(i => Note.Normalize(i) == stringNotes[s][pos]))
                    {
                        positionsPerString[s].Add(pos);
                    }
                }
            }

            var layouts = GetPositionsForStrings(positionsPerString, stringNotes, info, 0).ToArray();
#if TESTING
            var k = 0;
            layouts = layouts.Skip(k).ToArray();
#endif
            var guitarChordTypes = instrument != Instrument.Guitar
                ? new List<GuitarChordType> { GuitarChordType.Standard }
                : Enum.GetValues(typeof(GuitarChordType))
                      .Cast<GuitarChordType>()
                      .Where(i => i != GuitarChordType.Standard &&
                                  (allowSpecial || i != GuitarChordType.Special) &&
                                  (allowBarre || (i != GuitarChordType.SixStringBarre &&
                                                  i != GuitarChordType.FiveStringBarre)))
                      .ToList();
#if !TESTING
            Parallel.ForEach(layouts, layout =>
#else
            foreach(var layout in layouts)
#endif
            {
                foreach (var guitarChordType in guitarChordTypes)
                {
                    var result = new GuitarChordLayout(chord, layout, guitarChordType, instrument);
                    var b = result.IsValid(allowPartial) &&
                            result.Fret <= maxFret;
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

        private static IEnumerable<int[]> GetPositionsForStrings(HashSet<int>[] positionsPerString, Note[][] stringNotes, InstrumentInfo info, int s)
        {
            var count = positionsPerString.Length;
            var guitar = info == InstrumentInfo.Guitar;

            if (s == count - 1)
            {
                foreach (var pos in positionsPerString[s])
                {
					var positions = Enumerable.Repeat(X, count).ToArray();

                    positions[s] = pos;
                    yield return positions;
                }
                yield break;
            }
            foreach (var pos in positionsPerString[s])
            {
                var positions0 = GetPositionsForStrings(positionsPerString, stringNotes, info, s + 1);

                foreach (var pos0 in positions0)
                {
                    var positions = new int[count];

                    pos0.CopyTo(positions, 0);
                    positions[s] = pos;
                    //TODO: implement 1st 3 checks from IsValid() here
                    //(guitars may only mute strings at the bass side or a single string in the middle);
                    //a short drone string may double its neighbour
                    if ((pos == X && (!guitar || s == 0 || positions[s + 1] != X)) ||
                        (pos != X && (positions[s + 1] == X || info.IsShortString(s) || info.IsShortString(s + 1) ||
                                      stringNotes[s+1][positions[s + 1]] != stringNotes[s][positions[s]])))
                    {
                        yield return positions;
                    }
                }
            }
		}

        private Note[] GetUsedNotes()
        {
            var stringNotes = StringNotes(Instrument);
            var notesUsed = new List<Note>(Positions.Length);

			for (var s = 0; s < Positions.Length; s++)
			{
				if (Positions[s] < 0)
				{
					continue;
				}
                var note = stringNotes[s][Positions[s]];
                var noteIndex = Array.IndexOf(Chord.NormalizedNotes, note);

                if (noteIndex >= 0)
				{
					notesUsed.Add(Chord.Notes[noteIndex]);
				}
			}

            return notesUsed.ToArray();
		}

        /// <summary>Have we managed to find all the notes from the given chord? If not we may leave out one non-mandatory note.</summary>
        private bool HasChordNotes(bool allowPartial)
        {
            return !((!allowPartial &&
                      Chord.Notes.Any(i => !Notes.Contains(i))) ||
                     Chord.Notes.Any(i => !Notes.Contains(i) &&
                                          !Chord.NonMandatoryNotes.Contains(i)) ||
                     Chord.Notes.Count(i => !Notes.Contains(i) &&
                                            Chord.NonMandatoryNotes.Contains(i)) > 1);
        }

        /// <summary>Rules for instruments other than the guitar (4 strings, so every string has its own finger).</summary>
        private bool IsValidGeneric(bool allowPartial)
        {
            if (!HasChordNotes(allowPartial))
            {
                return false;
            }
            //do not allow same notes on neighbouring strings, except next to a short drone string (banjo's 5th),
            //which rings along in most banjo chords
            var info = InstrumentInfo;
            var stringNotes = StringNotes(Instrument);
            for (var s = 0; s < Positions.Length - 1; s++)
            {
                if (Positions[s] != X && Positions[s + 1] != X &&
                    !info.IsShortString(s) && !info.IsShortString(s + 1) &&
                    stringNotes[s][Positions[s]] == stringNotes[s + 1][Positions[s + 1]])
                {
                    return false;
                }
            }
            //distance between positions should not exceed 4 frets
            var usedPositions = Positions.Where(i => i > 0).ToArray();
            if (usedPositions.Length > 0 && usedPositions.Max() - usedPositions.Min() + 1 > 4)
            {
                return false;
            }
            //four fingers, the index finger may hold a barre across the lowest fret
            if (usedPositions.Length > 4 &&
                usedPositions.Length - usedPositions.Count(i => i == usedPositions.Min()) + 1 > 4)
            {
                return false;
            }
            //silent strings are allowed only at the edges
            var played = Positions.Select((pos, idx) => new { pos, idx }).Where(i => i.pos != X).Select(i => i.idx).ToArray();
            if (played.Length < 2 ||
                played.Last() - played.First() + 1 != played.Length)
            {
                return false;
            }
            return true;
        }

        private bool IsValid(bool allowPartial)
        {
            if (Instrument != Instrument.Guitar)
            {
                return IsValidGeneric(allowPartial);
            }
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
                 GuitarChordType == GuitarChordType.Special
                ) &&
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

            return Instrument == other.Instrument && Positions.SequenceEqual(other.Positions);
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
