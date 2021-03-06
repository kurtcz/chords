﻿﻿﻿﻿﻿using System;
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
    [Activity(Label = "Guitar Chords", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {
        private static Bundle _bundle;
        private WebView _webView;
        private HybridWebViewClient _viewClient;
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
            _viewClient = new HybridWebViewClient();
            _webView.SetWebViewClient(_viewClient);
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
                _viewClient.ShowChord(_webView, new NameValueCollection());
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

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            if (e.Action == KeyEventActions.Down)
            {
                if (keyCode == Keycode.Menu)
                {
                    _viewClient.Settings(_webView, new NameValueCollection());

                    return true;
                }
                else if (keyCode == Keycode.Back &&
                    _webView.CanGoBack())
                {
                    _webView.GoBack();

                    return true;
                }
            }

            return base.OnKeyDown(keyCode, e);
        }

        private class HybridWebViewClient : WebViewClient
        {
			private Note _currentRoot;
			private ChordType _currentChordType;
            private bool _allowPartial;
            private SettingsModel _currentSettings = SettingsModel.Load();
            private Note[] _allRoots;
            private string[] _allChordTypes;

            public override void OnReceivedError(WebView view, IWebResourceRequest request, WebResourceError error)
            {
				//Hack to handle $.ajax('hybrid:xxx'); calls that are failing with ERR_UNKNOWN_URL_SCHEME
                //Alternatively we could also override ShouldInterceptRequest()
				var scheme = "hybrid:";
                var url = request.Url.ToString();

                if (url.StartsWith(scheme))
                {
                    //fetch the page in the background thread as this is an ajax call so we do not want to block the webview
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
                {
                    return false;
                }

                // This handler will treat everything between the protocol and "?"
                // as the method name.  The querystring has all of the parameters.
                var resources = url.Substring(scheme.Length).Split('?');
                var method = resources[0];
                var parameters = resources.Length > 1 ? System.Web.HttpUtility.ParseQueryString(resources[1]) : new NameValueCollection();

				webView.ScrollbarFadingEnabled = true;  //default value unless changed
				if (method == "ShowChord")
                {
                    ShowChord(webView, parameters);
                }
				else if (method == "ShowChordResults")
				{
                    ShowChordResults(webView, parameters);
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
				else if (method == "Settings")
				{
					Settings(webView, parameters);
				}
				else
                {
                    return false;
                }
				return true;
            }

            public void ShowChord(WebView webView, NameValueCollection parameters)
            {
                string page;
                var model = new ShowChordModel
                {
                    Root = _currentRoot,
                    ChordType = _currentChordType,
                    conv = _currentSettings.conv,
                    ShowBasicChordTypes = _currentSettings.ShowBasicChordTypes
                };
                model.Populate(parameters);

				_allRoots = null;
				_allChordTypes = null;

				var template = new ShowChordView { Model = model };

                page = template.GenerateString();
                // Load the rendered HTML into the view with a base URL 
                // that points to the root of the bundled Assets folder
                webView.LoadDataWithBaseURL("file:///android_asset/?page=ShowChord", page, "text/html", "UTF-8", null);
            }

			public void ShowChordResults(WebView webView, NameValueCollection parameters)
            {
				string page;
				var model = new ShowChordModel
				{
					Root = _currentRoot,
					ChordType = _currentChordType,
					conv = _currentSettings.conv,
					ShowBasicChordTypes = _currentSettings.ShowBasicChordTypes
				};
				model.Populate(parameters);

				Chord chord;
				var id = string.Format("{0}{1}", model.Root, model.ChordType.ToDescription());
				var resultModel = new ShowChordResultModel
				{
					conv = _currentSettings.conv,
                    AllowPartial = _allowPartial,
					AllRoots = _allRoots,
					AllChordTypes = _allChordTypes
				};

				if (!string.IsNullOrEmpty(id))
				{
					resultModel.Populate(parameters);

					if (!Chord.TryParse(id, NamingConvention.English, out chord))
					{
						resultModel.Error = $"{id} is not a valid chord in {_currentSettings.conv} naming convention";
					}
					else
					{
						_currentRoot = chord.Root;
						_currentChordType = chord.ChordType;
                        _allowPartial = resultModel.AllowPartial;
						resultModel.Root = _currentRoot;
						resultModel.ChordType = _currentChordType.ToDescription();
						resultModel.ChordDecorator = new ChordDecorator(chord, _currentSettings.conv);
					}
					var template = new ShowChordResultView { Model = resultModel };

					page = template.GenerateString();
				}
                else
                {
					var template = new ShowChordView { Model = model };

					page = template.GenerateString();
				}

				webView.LoadDataWithBaseURL("file:///android_asset/?page=ShowChordResults", page, "text/html", "UTF-8", null);
            }

			public void ShowChordLayouts(WebView webView, NameValueCollection parameters)
            {
                var model = new ShowChordLayoutsModel
				{
                    Root = _currentRoot,
                    ChordType = _currentChordType.ToDescription(),
                    conv = _currentSettings.conv,
                    AllowPartial = _allowPartial,
                    AllowSpecial = _currentSettings.AllowSpecial,
					AllRoots = _allRoots,
					AllChordTypes = _allChordTypes,
                    ShowTips = _currentSettings.ShowTips,
				};
                model.Populate(parameters);

				var id = string.Format("{0}{1}", model.Root, model.ChordType);
				Chord chord;

                if (!Chord.TryParse(id, NamingConvention.English, out chord))
				{
					model.Error = $"{id} is not a valid chord";
				}
				else
				{
					model.ChordDecorator = new ChordDecorator(chord, _currentSettings.conv);
                    model.Layouts = model.ChordDecorator
                        .GenerateLayouts(_currentSettings.AllowBarre, _currentSettings.AllowSpecial, model.AllowPartial, _currentSettings.maxFret)
                        .OrderBy(i => i.Favorite ? 0 : 1)
                        .ToArray();
                }
				var template = new ShowChordLayoutsView { Model = model };
                var page = template.GenerateString();

                //webView.ScrollbarFadingEnabled = false;
				webView.LoadDataWithBaseURL("file:///android_asset/?page=ShowChordLayouts", page, "text/html", "UTF-8", null);
            }

			public void FindChord(WebView webView, NameValueCollection parameters)
			{
                string page;

                if (string.IsNullOrWhiteSpace(parameters["note"]))
                {
                    var chordParams = new ChordParams(parameters);
                    var model = new FindChordModel
                    {
                        conv = _currentSettings.conv, 
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
                    var notes = tokens.Select(i => Note.TryParse(i, _currentSettings.conv, out note) ? note : null)
                                      .Where(i => i != null)
                                      .ToArray();
                    var chords = Chord.Find(notes, chordParams.Strict);

                    _allowPartial = true;
                    if (chords.Length > 0)
                    {
                        var chord = chords.First();

                        _currentRoot = chord.Root;
                        _currentChordType = chord.ChordType;
                        if (chords.Length > 1)
                        {
                            _allRoots = chords.Select(i => i.Root).ToArray();
                            _allChordTypes = chords.Select(i => i.ChordType.ToDescription()).ToArray();
                        }
                        else
                        {
                            _allRoots = null;
                            _allChordTypes = null;
                        }

						parameters.Add("partial", "true");
						ShowChordResults(webView, parameters);
                        return;
                    }
					var model = new FindChordModel
                    {
                        conv = _currentSettings.conv, 
                        Strict = chordParams.Strict,
                        SelectedNotes = notes.Select(i => i.ToString(_currentSettings.conv))
                                             .ToArray(),
                        Error = "No chord found"
                    };
					var template = new FindChordView() { Model = model };

					page = template.GenerateString();
				}

				webView.LoadDataWithBaseURL("file:///android_asset/?page=FindChord", page, "text/html", "UTF-8", null);
			}

            public void PlayChord(WebView webView, NameValueCollection parameters)
            {
                var chordParams = new ChordParams(parameters);
                var id = parameters["id"];
                float volume;

                if (!float.TryParse(parameters["volume"], out volume))
                {
                    volume = 1f;
                }
                if (!string.IsNullOrEmpty(id))
                {
                    Chord chord;

                    if (!Chord.TryParse(id, NamingConvention.English, out chord))
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

                        _soundPool.Play(_soundIds[1], volume, volume, 0, 0, playRate);
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

							_soundPool.Play(_soundIds[i], volume, volume, 0, 0, playRate);
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
                var model = new CircleModel(parameters)
                {
                    conv = _currentSettings.conv
                };
				var template = new CircleView() { Model = model };
				var page = template.GenerateString();

                webView.LoadDataWithBaseURL("file:///android_asset/?page=CircleOfFifths", page, "text/html", "UTF-8", null);
			}

            public void AddToFavorites(WebView webView, NameValueCollection parameters)
            {
                var chordParams = new ChordParams(parameters);
                var tokens = parameters["positions"].Split(',');
                var positions = tokens.Select(i => int.Parse(i)).ToArray();
                var id = $"{chordParams.Root}{chordParams.ChordType}";
                Chord chord;

                if (!Chord.TryParse(id, _currentSettings.conv, out chord))
                {
                    return;
                }
                try
                {
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
                catch(Exception e)
                {
                    System.Diagnostics.Debug.WriteLine($"Cannot save favorites\n{e.Message}");
                }
			}

            public void RemoveFromFavorites(WebView webView, NameValueCollection parameters)
            {
				var chordParams = new ChordParams(parameters);
				var tokens = parameters["positions"].Split(',');
				var positions = tokens.Select(i => int.Parse(i)).ToArray();
				var id = $"{chordParams.Root}{chordParams.ChordType}";
				Chord chord;

                if (!Chord.TryParse(id, _currentSettings.conv, out chord))
				{
					return;
				}
				if (!Favorites.Chords.ContainsKey(chord))
				{
					return;
				}
                try
                {
                    var layout = new GuitarChordLayout(chord, positions);
                    Favorites.Chords[chord].Remove(layout);
                    Favorites.Save();

                    var js = string.Format("setFavorite('.chord-layout[data-positions=\"{0}\"]', '{1}');", parameters["positions"], false);
                    webView.LoadUrl("javascript:" + js);
                }
				catch (Exception e)
				{
					System.Diagnostics.Debug.WriteLine($"Cannot save favorites\n{e.Message}");
				}
			}

            public void FavoriteChords(WebView webView, NameValueCollection parameters)
            {
                var model = new FavoriteChordsModel
                {
                    conv = _currentSettings.conv
                };

                model.Chords = Favorites.Chords
                                        .Select(kvp => new KeyValuePair<ChordDecorator, GuitarChordLayoutDecorator[]>(
                                                            new ChordDecorator(kvp.Key, model.conv),
                                                            kvp.Value.Select(i => new GuitarChordLayoutDecorator(i, model.conv)).ToArray()))
                                        .Where(i => i.Value.Any())
                                        .OrderBy(i => i.Key.Root.Tone)
                                        .ThenBy(i => i.Key.Root.Accidental)
										.ThenBy(i => i.Key.ChordType);
				var template = new FavoriteChordsView() { Model = model };
				var page = template.GenerateString();

				webView.LoadDataWithBaseURL("file:///android_asset/?page=FavoriteChords", page, "text/html", "UTF-8", null);
            }

            public void Settings(WebView webView, NameValueCollection parameters)
			{
                _currentSettings.Populate(parameters);
                _currentSettings.Save();

                if (!string.IsNullOrEmpty(parameters["silent"]))
                {
                    return;
                }
                var template = new SettingsView() { Model = _currentSettings };
				var page = template.GenerateString();

				webView.LoadDataWithBaseURL("file:///android_asset/?page=Settings", page, "text/html", "UTF-8", null);
			}
		}
    }
}
