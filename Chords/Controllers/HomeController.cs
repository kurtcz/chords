using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Chords.Models;
using Chords.ViewModels;

namespace Chords.Controllers
{
    public class HomeController : Controller
    {
        private IMemoryCache _cache;

        public HomeController(IMemoryCache cache)
        {
            _cache = cache;
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
			if (parameters == null || string.IsNullOrWhiteSpace(parameters.root))
			{
				return RedirectToAction("Index");
			}

			try
			{
				var id = $"{parameters.root}{parameters.@type}";
				var chord = Chord.Parse(id, parameters.conv);

                ViewData["parameters"] = parameters;

				return View(chord);
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

            if (Request.Headers["x-requested-with"] != "XMLHttpRequest")
            {
                return BadRequest();
            }

            var cacheId = parameters.ToString();
			var result = _cache.GetOrCreate(cacheId, entry =>
			{
				try
				{
                    var chordSymbol = $"{parameters.root}{parameters.@type}";
                    var chord = Chord.Parse(chordSymbol, parameters.conv);
					var chordDecorator = new ChordDecorator(chord, parameters.special, parameters.@partial);

					return chordDecorator;
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

		public IActionResult Error()
        {
            return View();
        }
    }
}
