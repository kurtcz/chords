using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Chords.Core.Models;
using Chords.ViewModels;
using System.Linq;
using Chords.Core.Extensions;
using Microsoft.AspNetCore.Http;

namespace Chords.Controllers
{
    public class HomeController : Controller
    {
        private IMemoryCache _cache;

        public HomeController(IMemoryCache cache)
        {
            _cache = cache;
		}

        private bool IsAjax()
        {
            return Request.Headers["x-requested-with"] == "XMLHttpRequest";
		}

		public IActionResult Index([FromQuery] NamingConvention conv)
        {
            ViewData["conv"] = Helper.NamingConventionList(conv);
            ViewData["root"] = Helper.Notes[conv];
            ViewData["type"] = Helper.ChordTypeList;

            return View();
        }

        public IActionResult ShowChord([FromQuery] ShowChordParams parameters)
		{
			if (string.IsNullOrWhiteSpace(parameters.root))
			{
				return RedirectToAction("Index");
			}

			try
			{
				var id = $"{parameters.root}{parameters.@type}";
				Chord chord;

				if (!Chord.TryParse(id, parameters.conv, out chord))
                {
                    TempData["ErrorMessage"] = $"{id} is not a valid chord";
					return RedirectToAction("Index");
                }

                var chordDecorator = new ChordDecorator(chord, parameters.conv);

                parameters.root = chord.Root.ToString(parameters.conv);
                parameters.@type = chord.ChordType.ToDescription();
                ViewData["parameters"] = parameters;

				return View(chordDecorator);
			}
			catch (Exception ex)
			{
				TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index");
			}
		}

        public IActionResult ShowChordLayouts([FromQuery] ShowChordParams parameters)
        {
			if (string.IsNullOrWhiteSpace(parameters.root))
			{
                return BadRequest();
			}
            if (!IsAjax())
			{
				return BadRequest();
			}

			var cacheId = parameters.ToString();
			var result = _cache.GetOrCreate(cacheId, entry =>
			{
				try
				{
                    var chordSymbol = $"{parameters.root}{parameters.@type}";
                    Chord chord;

                    if (!Chord.TryParse(chordSymbol, parameters.conv, out chord))
                    {
                        return null;
                    }
                    var chordDecorator = new ChordDecorator(chord, parameters.conv);
                    var layouts = chordDecorator.GenerateLayouts(parameters.special, parameters.@partial);

                    return layouts;
				}
				catch (Exception)
				{
					return null;
				}
			});
			if (result == null)
			{
                return BadRequest();
			}

            return PartialView(result);
        }

        public IActionResult FindChord([FromQuery] string sequence, [FromQuery] NamingConvention conv, [FromQuery] bool strict)
        {
            ViewData["conv"] = Helper.NamingConventionList(conv);
            if (sequence == null)
            {
                if (IsAjax())
                {
                    return new EmptyResult();
                }
                return View("FindChord", string.Empty);
            }

            var tokens = sequence.Split(new [] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            Note note;
            var notes = tokens.Select(i => Note.TryParse(i, conv, out note) ? note : null)
                              .Where(i => i != null)
                              .ToArray();
            var chords = Chord.Find(notes, strict);

			if (!chords.Any())
			{
                return StatusCode(StatusCodes.Status404NotFound, "No chord found");
			}
			
            var chordDecorator = chords[0] != null ? new ChordDecorator(chords[0], conv) : null;

            ViewData["sequence"] = sequence;
			ViewData["parameters"] = new ShowChordParams
			{
                root = chords[0].Root.ToString(conv),
                @type = chords[0].ChordType.ToDescription(),
                @partial = true,
                conv = conv
			};
			if (IsAjax())
            {
                return PartialView("ShowChord", chordDecorator);
            }
            return View("FindChord", sequence);
        }

		public IActionResult Error()
        {
            return View();
        }
    }
}
