using System;
using System.Linq;
using Chords.Core.Models;
using Xunit;
using Xunit.Abstractions;
namespace Chords.Core.Tests;
public class TmpHardTests
{
    private readonly ITestOutputHelper _o;
    public TmpHardTests(ITestOutputHelper o) { _o = o; }
    [Fact]
    public void ListHard()
    {
        foreach (var inst in new[] { Instrument.Guitar, Instrument.Ukulele, Instrument.Mandolin, Instrument.BassGuitar, Instrument.Banjo })
        {
            int total = 0, hard = 0, allHard = 0; var ex = new System.Collections.Generic.List<string>();
            foreach (ChordType t in Enum.GetValues(typeof(ChordType)))
            foreach (var n in Enumerable.Range(0,12).Select(p => Note.FromPitch(60+p)))
            {
                Chord c; try { c = new Chord(n, t); } catch { continue; }
                var ls = GuitarChordLayout.Generate(c, inst, true, true, true, 12).ToArray();
                if (ls.Length == 0) continue;
                total++;
                var h = ls.Where(l => l.DifficultyLevel == DifficultyLevel.Hard).ToArray();
                if (h.Length > 0) hard++;
                if (ls.All(l => l.DifficultyLevel == DifficultyLevel.Hard)) { allHard++; ex.Add($"{n}{t}"); }
                if (h.Length > 0 && ex.Count < 3 && ex.Count >= 0 && n.ToString().StartsWith("C")) _o.WriteLine($"  {inst} {n} {t}: {h[0]} ({h[0].Difficulty})");
            }
            _o.WriteLine($"{inst}: chords {total}, with some hard {hard}, only hard {allHard}: {string.Join(", ", ex.Take(15))}");
        }
    }
}
