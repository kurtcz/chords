﻿using System;
using Android.App;
using Android.Content;
using Android.Media;
using Android.Runtime;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using Android.OS;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Linq;
using Chords.Android.Models;
using Chords.Android.Views;
using Chords.Core.Extensions;
using Chords.Core.Models;
using System.Threading;
using System.Collections.Generic;

namespace Chords.Android
{
    [Activity(Label = "Chords", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {
        private static Bundle _bundle;
        private WebView _webView;
		private static SoundPool _soundPool;
        private static int[] _soundIds;
        private static int[] _nextStringPosition =
        {
            5,
            5,
            5,
            4,
            4,
            int.MaxValue
        };

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Favorites.Init();
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

			_webView = FindViewById<WebView>(Resource.Id.webView);
            _webView.Settings.JavaScriptEnabled = true;

            // Use subclassed WebViewClient to intercept hybrid native calls
            var viewClient = new HybridWebViewClient();
            _webView.SetWebViewClient(viewClient);
            _webView.LongClick += (object sender, View.LongClickEventArgs e) => { e.Handled = true; };
            if (_soundPool == null)
            {
                _soundPool = new SoundPool(6, Stream.Music, 0);
                _soundIds = new int[6];
                _soundIds[0] = _soundPool.Load(Application, Resource.Raw.E6, 1);
                _soundIds[1] = _soundPool.Load(Application, Resource.Raw.A5, 1);
                _soundIds[2] = _soundPool.Load(Application, Resource.Raw.D4, 1);
                _soundIds[3] = _soundPool.Load(Application, Resource.Raw.G3, 1);
                _soundIds[4] = _soundPool.Load(Application, Resource.Raw.B2, 1);
                _soundIds[5] = _soundPool.Load(Application, Resource.Raw.E1, 1);
            }
            if (_bundle == null)
            {
                viewClient.ShowChord(_webView, new NameValueCollection());
            }
            else
            {
                _webView.RestoreState(_bundle);
            }
        }

        protected override void OnPause()
        {
            base.OnPause();

            _bundle = new Bundle();
            _webView.SaveState(_bundle);
        }

        private class HybridWebViewClient : WebViewClient
        {
			public override void OnReceivedError(WebView view, IWebResourceRequest request, WebResourceError error)
            {
                //Hack to handle $.ajax('hybrid:xxx'); calls that are failing with ERR_UNKNOWN_URL_SCHEME
				var scheme = "hybrid:";
                var url = request.Url.ToString();

                if (url.StartsWith(scheme))
                {
                    Task.Run(() => ShouldOverrideUrlLoading(view, url));
                }
                else
                {
                    base.OnReceivedError(view, request, error);
                }
            }

            public override bool ShouldOverrideUrlLoading(WebView webView, string url)
            {
                // If the URL is not our own custom scheme, just let the webView load the URL as usual
                var scheme = "hybrid:";

                if (!url.StartsWith(scheme))
                    return false;

                // This handler will treat everything between the protocol and "?"
                // as the method name.  The querystring has all of the parameters.
                var resources = url.Substring(scheme.Length).Split('?');
                var method = resources[0];
                var parameters = resources.Length > 1 ? System.Web.HttpUtility.ParseQueryString(resources[1]) : new NameValueCollection();

				if (method == "ShowChord")
                {
                    ShowChord(webView, parameters);
                }
				else if (method == "ShowChordLayouts")
				{
					ShowChordLayouts(webView, parameters);
				}
				else if (method == "FindChord")
				{
					FindChord(webView, parameters);
				}
                else if (method == "PlayChord")
                {
					PlayChord(webView, parameters);
                }
				else if (method == "Circle")
				{
                    CircleOfFifths(webView, parameters);
				}
                else if (method == "AddToFavorites")
                {
                    AddToFavorites(webView, parameters);
                }
				else if (method == "RemoveFromFavorites")
				{
					RemoveFromFavorites(webView, parameters);
				}
                else if (method == "FavoriteChords")
                {
                    FavoriteChords(webView, parameters);
                }
                else
                {
                    return false;
                }
				return true;
            }

            private Note _currentRoot;
            private ChordType _currentChordType;

            public void ShowChord(WebView webView, NameValueCollection parameters)
            {
				string page;
				var showChordParams = new ChordParams(parameters);
                var model = new ShowChordModel { Root = _currentRoot, ChordType = _currentChordType, conv = showChordParams.NamingConvention };
                var resultModel = new ShowChordResultModel
                {
                    Parameters = parameters
                };

                if (!string.IsNullOrEmpty(showChordParams.Root))
                {
					var id = $"{showChordParams.Root}{showChordParams.ChordType}";
					Chord chord;

					if (!Chord.TryParse(id, showChordParams.NamingConvention, out chord))
					{
                        resultModel.Error = $"{id} is not a valid chord in {showChordParams.NamingConvention} naming convention";
					}
                    else
                    {
                        _currentRoot = chord.Root;
                        _currentChordType = chord.ChordType;
						resultModel.ChordDecorator = new ChordDecorator(chord, showChordParams.NamingConvention);
					}
					var template = new ShowChordResultView { Model = resultModel };
					
                    page = template.GenerateString();
				}
                else
                {
					var template = new ShowChordView { Model = model };					

                    page = template.GenerateString();
				}
				// Load the rendered HTML into the view with a base URL 
				// that points to the root of the bundled Assets folder
				webView.LoadDataWithBaseURL("file:///android_asset/?page=1", page, "text/html", "UTF-8", null);
			}

			public void ShowChordLayouts(WebView webView, NameValueCollection parameters)
            {
				var showChordParams = new ChordParams(parameters);
				var model = new ShowChordLayoutsModel
				{
					Parameters = parameters
				};

				var id = $"{showChordParams.Root}{showChordParams.ChordType}";
				Chord chord;

				if (!Chord.TryParse(id, showChordParams.NamingConvention, out chord))
				{
					model.Error = $"{id} is not a valid chord";
				}
				else
				{
					model.ChordDecorator = new ChordDecorator(chord, showChordParams.NamingConvention);
                    model.Layouts = model.ChordDecorator
                                         .GenerateLayouts(showChordParams.Special, showChordParams.Partial)
                        .OrderBy(i => i.Favorite ? 0 : 1)
                        .ToArray();
                }
				var template = new ShowChordLayoutsView { Model = model };
                var page = template.GenerateString();

                // Load the rendered HTML into the view with a base URL 
				// that points to the root of the bundled Assets folder
				webView.LoadDataWithBaseURL("file:///android_asset/", page, "text/html", "UTF-8", null);
            }

			public void FindChord(WebView webView, NameValueCollection parameters)
			{
                string page;

                if (string.IsNullOrWhiteSpace(parameters["note"]))
                {
                    var chordParams = new ChordParams(parameters);
                    var model = new FindChordModel
                    {
                        conv = chordParams.NamingConvention, 
                        Strict = chordParams.Strict
                    };
                    var template = new FindChordView() { Model = model };

                    page = template.GenerateString();
				}
                else
                {
                    Note note;
					var chordParams = new ChordParams(parameters);
					var tokens = parameters["note"].Split(',');
                    var notes = tokens.Select(i => Note.TryParse(i, chordParams.NamingConvention, out note) ? note : null)
                                      .Where(i => i != null)
                                      .ToArray();

                    if (!notes.Any())
                    {
						notes = tokens.Select(i => Note.TryParse(i, chordParams.OldNamingConvention, out note) ? note : null)
									  .Where(i => i != null)
									  .ToArray();
					}
                    var chord = Chord.Find(notes, chordParams.Strict);

                    if (chord != null)
                    {
                        parameters.Add("root", chord.Root.ToString(chordParams.NamingConvention));
                        parameters.Add("type", chord.ChordType.ToDescription());
                        parameters.Add("partial", "true");
                        ShowChord(webView, parameters);
                        return;
                    }
					var model = new FindChordModel
                    {
                        conv = chordParams.NamingConvention, 
                        Strict = chordParams.Strict,
                        SelectedNotes = notes.Select(i => i.ToString(chordParams.NamingConvention))
                                             .ToArray(),
                        Error = "No chord found"
                    };
					var template = new FindChordView() { Model = model };

					page = template.GenerateString();
				}

				// Load the rendered HTML into the view with a base URL 
				// that points to the root of the bundled Assets folder
				webView.LoadDataWithBaseURL("file:///android_asset/", page, "text/html", "UTF-8", null);
			}

            public void PlayChord(WebView webView, NameValueCollection parameters)
            {
                var chordParams = new ChordParams(parameters);
                var id = parameters["id"];

                if (!string.IsNullOrEmpty(id))
                {
                    Chord chord;

                    if (!Chord.TryParse(id, chordParams.NamingConvention, out chord))
                    {
                        return;
                    }
					var c = new Note(Core.Models.Tone.C);
                    var minDistance = 3 + c.ChromaticDistance(chord.Notes[0]);

					foreach(var note in chord.Notes)
                    {
						var pos = 3 + c.ChromaticDistance(note);

                        if (pos < minDistance)
                        {
                            pos += 12;
                        }
                        var playRate = Note.HalfStepsToPlayRate(pos);

                        _soundPool.Play(_soundIds[1], 1, 1, 0, 0, playRate);
                        Thread.Sleep(50);
                    }
                    return;
                }
                var intPositions = parameters["positions"]?.Split(',')?.Select(i => int.Parse(i))?.ToArray();

                if (intPositions == null)
                {
                    return;
                }
                for (var n = 0; n < 2; n++)
                {
					for (var i = 0; i < intPositions.Length; i++)
					{
						var halfSteps = intPositions[i];

						if (halfSteps >= 0)
						{
                            var playRate = Note.HalfStepsToPlayRate(halfSteps);

							_soundPool.Play(_soundIds[i], 1, 1, 0, 0, playRate);
                            if (n == 0)
                            {
								Thread.Sleep(200);
							}
                            else
                            {
								Thread.Sleep(50);
							}
						}
					}
                    Thread.Sleep(200);
				}
			}

            public void CircleOfFifths(WebView webView, NameValueCollection parameters)
            {
                var model = new CircleModel(parameters);
				var template = new CircleView() { Model = model };
				var page = template.GenerateString();
				// Load the rendered HTML into the view with a base URL 
				// that points to the root of the bundled Assets folder
				webView.LoadDataWithBaseURL("file:///android_asset/", page, "text/html", "UTF-8", null);
			}

            public void AddToFavorites(WebView webView, NameValueCollection parameters)
            {
                var chordParams = new ChordParams(parameters);
                var tokens = parameters["positions"].Split(',');
                var positions = tokens.Select(i => int.Parse(i)).ToArray();
                var id = $"{chordParams.Root}{chordParams.ChordType}";
                Chord chord;

                if (!Chord.TryParse(id, chordParams.NamingConvention, out chord))
                {
                    return;
                }
                var layout = new GuitarChordLayout(chord, positions);
                if (!Favorites.Chords.ContainsKey(chord))
                {
                    Favorites.Chords.Add(chord, new HashSet<GuitarChordLayout>());
                }
                Favorites.Chords[chord].Add(layout);
                Favorites.Save();

                var js = string.Format("setFavorite('.chord-layout[data-positions=\"{0}\"]', '{1}');", parameters["positions"], true);
                webView.LoadUrl("javascript:" + js);
			}

            public void RemoveFromFavorites(WebView webView, NameValueCollection parameters)
            {
				var chordParams = new ChordParams(parameters);
				var tokens = parameters["positions"].Split(',');
				var positions = tokens.Select(i => int.Parse(i)).ToArray();
				var id = $"{chordParams.Root}{chordParams.ChordType}";
				Chord chord;

				if (!Chord.TryParse(id, chordParams.NamingConvention, out chord))
				{
					return;
				}
				var layout = new GuitarChordLayout(chord, positions);
				if (!Favorites.Chords.ContainsKey(chord))
				{
                    return;
				}
                Favorites.Chords[chord].Remove(layout);
                Favorites.Save();

                var js = string.Format("setFavorite('.chord-layout[data-positions=\"{0}\"]', '{1}');", parameters["positions"], false);
				webView.LoadUrl("javascript:" + js);
			}

            public void FavoriteChords(WebView webView, NameValueCollection parameters)
            {
                var model = new FavoriteChordsModel(parameters);

                model.Chords = Favorites.Chords
                                        .Select(kvp => new KeyValuePair<ChordDecorator, GuitarChordLayoutDecorator[]>(
                                                            new ChordDecorator(kvp.Key, model.conv),
                                                            kvp.Value.Select(i => new GuitarChordLayoutDecorator(i, model.conv)).ToArray()))
                                        .OrderBy(i => i.Key.Root.Tone)
                                        .ThenBy(i => i.Key.Root.Accidental)
										.ThenBy(i => i.Key.ChordType);
				var template = new FavoriteChordsView() { Model = model };
				var page = template.GenerateString();

				// Load the rendered HTML into the view with a base URL 
				// that points to the root of the bundled Assets folder
				webView.LoadDataWithBaseURL("file:///android_asset/", page, "text/html", "UTF-8", null);
            }

            public bool LongClick(View v)
            {
                return true;
            }
		}
    }
}
