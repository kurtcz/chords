using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Chords.Core.Extensions;
using Chords.Core.Models;

namespace Chords.Android.Models
{
    public class CircleModel
    {
        public readonly NamingConvention[] NamingConventions = Enum.GetValues(typeof(NamingConvention))
                                                                   .Cast<NamingConvention>()
                                                                   .ToArray();
        public Dictionary<NamingConvention, string[]> MajorCircle =
            Enum.GetValues(typeof(NamingConvention))
                .Cast<NamingConvention>()
                .SelectMany(i => Enumerable.Range(-8, 17)
                                           .Select(signature =>
                                           {
                                               var root = new Note(Tone.F, Accidental.Flat);
                                               Note note = root;

                                               for (var k = -8; k < signature; k++)
                                               {
                                                   note = note.NoteAtInterval(Interval.P5);
                                               }
                                               
                                                return new
                                               {
                                                   Key = i,
                                                   Value = note.ToString(i)
                                               };
                                            }))
                .GroupBy(g => g.Key)
                .ToDictionary(k => k.Key, v => v.Select(i => i.Value).ToArray());
        public Dictionary<NamingConvention, string[]> MinorCircle =
            Enum.GetValues(typeof(NamingConvention))
                .Cast<NamingConvention>()
                .SelectMany(i => Enumerable.Range(-8, 17)
                                           .Select(signature =>
                                           {
                                               var root = new Note(Tone.D, Accidental.Flat);
                                               Note note = root;

                                               for (var k = -8; k < signature; k++)
                                               {
                                                   note = note.NoteAtInterval(Interval.P5);
                                               }

                                               return new
                                               {
                                                   Key = i,
                                                   Value = $"{note.ToString(i)}m"
                                               };
                                           }))
                .GroupBy(g => g.Key)
                .ToDictionary(k => k.Key, v => v.Select(i => i.Value).ToArray());
        public Dictionary<NamingConvention, string[]> DiminishedCircle =
            Enum.GetValues(typeof(NamingConvention))
                .Cast<NamingConvention>()
                .SelectMany(i => Enumerable.Range(-8, 17)
                                           .Select(signature =>
                                           {
                                               var root = new Note(Tone.E, Accidental.Flat);
                                               Note note = root;

                                               for (var k = -8; k < signature; k++)
                                               {
                                                   note = note.NoteAtInterval(Interval.P5);
                                               }

                                               return new
                                               {
                                                   Key = i,
                                                   Value = $"{note.ToString(i)}°"
                                               };
                                           }))
                .GroupBy(g => g.Key)
                .ToDictionary(k => k.Key, v => v.Select(i => i.Value).ToArray());

        public NamingConvention conv { get; set; }
        public int index { get; set; }
        public string Signature { get; set; }
        public string Error { get; set; }

        public CircleModel(NameValueCollection parameters)
        {
            NamingConvention conv;
            int index;

            if (!Enum.TryParse(parameters["conv"], out conv))
            {
                conv = NamingConvention.English;
            }
            this.conv = conv;
            if (!int.TryParse(parameters["index"], out index))
            {
                index = 8;
            }
            if (index < 1 )
            {
                index += 12;
            }
            if (index >= MajorCircle[NamingConvention.English].Length - 1)
            {
                index -= 12;
            }
            this.index = index;

            Signature = string.Empty;
            if (index < 8)
            {
                Signature = $"{8 - index}b";
            }
            else if (index > 8)
            {
                Signature = $"{index - 8}#";
            }
        }
    }
}
