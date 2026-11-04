using System;
using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.Media;
using Android.Views;
using Android.Webkit;
using Android.Window;
using Android.OS;
using Java.Interop;
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
    [Activity(
        Name = "com.tnemec.chords.android.MainActivity",
        Label = "Guitar Chords",
        MainLauncher = true,
        Exported = true,
        Icon = "@mipmap/icon",
        Theme = "@android:style/Theme.Material.Light.NoActionBar",
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MainActivity : Activity
    {
        private const string HybridScheme = "hybrid:";
        private static readonly Color NavbarColor = Color.ParseColor("#222222");

        private static Bundle _bundle;
        private WebView _webView;
        private HybridWebViewClient _viewClient;
        private BackCallback _backCallback;
        private bool _backCallbackRegistered;
        // Pages are loaded with LoadDataWithBaseURL, so history items carry no usable URL or title;
        // remember which page each history index holds instead.
        private readonly System.Collections.Generic.Dictionary<int, string> _historyPages = new();
        private string _pendingPage;
		private static SoundPool _soundPool;
        private static int[] _soundIds;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

			Favorites.Init();
			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.Main);
            VolumeControlStream = global::Android.Media.Stream.Music;

			_webView = FindViewById<WebView>(Resource.Id.webView);
            _webView.Settings.JavaScriptEnabled = true;

            // Use subclassed WebViewClient to intercept hybrid native calls
            _viewClient = new HybridWebViewClient(this);
            _webView.SetWebViewClient(_viewClient);
            _webView.AddJavascriptInterface(new HybridBridge(_viewClient, _webView), HybridBridge.Name);
            _webView.LongClick += (object sender, View.LongClickEventArgs e) => { e.Handled = true; };

            SetupEdgeToEdge((View)_webView.Parent);
            SetupBackNavigation();

            if (_soundPool == null)
            {
                var audioAttributes = new AudioAttributes.Builder()
                    .SetUsage(AudioUsageKind.Media)
                    .SetContentType(AudioContentType.Music)
                    .Build();
                _soundPool = new SoundPool.Builder()
                    .SetMaxStreams(6)
                    .SetAudioAttributes(audioAttributes)
                    .Build();
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
                UpdateBackCallback();
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
                // Android 13+ delivers back through OnBackInvokedCallback (see SetupBackNavigation).
                else if (keyCode == Keycode.Back &&
                    !OperatingSystem.IsAndroidVersionAtLeast(33) &&
                    _webView.CanGoBack())
                {
                    GoBack();

                    return true;
                }
            }

            return base.OnKeyDown(keyCode, e);
        }

        /// <summary>
        /// Apps targeting API 35+ are drawn edge-to-edge on Android 15+, so pad the content
        /// to keep the fixed HTML navbar out from under the status and navigation bars.
        /// </summary>
        private static void SetupEdgeToEdge(View root)
        {
            root.SetBackgroundColor(NavbarColor);
            if (!OperatingSystem.IsAndroidVersionAtLeast(35))
            {
                return;
            }
            root.SetOnApplyWindowInsetsListener(new SystemBarsInsetsListener());
            root.RequestApplyInsets();
        }

        /// <summary>
        /// Apps targeting API 36 no longer receive KEYCODE_BACK on Android 16 (predictive back).
        /// A callback is registered only while the WebView has history, so that otherwise
        /// the system back-to-home animation is used.
        /// </summary>
        private void SetupBackNavigation()
        {
            if (!OperatingSystem.IsAndroidVersionAtLeast(33))
            {
                return;
            }
            _backCallback = new BackCallback(() =>
            {
                GoBack();
                UpdateBackCallback();
            });
        }

        /// <summary>
        /// The ShowChordResults page immediately ajax-loads ShowChordLayouts as a new history
        /// entry, so stepping back onto it would just bounce forward again; skip over it.
        /// </summary>
        private void GoBack()
        {
            var history = _webView.CopyBackForwardList();
            var index = history.CurrentIndex;
            var steps = -1;

            if (index >= 2 &&
                PageAt(index) == "ShowChordLayouts" &&
                PageAt(index - 1) == "ShowChordResults")
            {
                steps = -2;
            }
            if (_webView.CanGoBackOrForward(steps))
            {
                _webView.GoBackOrForward(steps);
            }
        }

        private string PageAt(int index) => _historyPages.TryGetValue(index, out var page) ? page : null;

        /// <summary>Called on the UI thread right before a page is loaded.</summary>
        internal void OnPageLoading(string pageName) => _pendingPage = pageName;

        internal void OnHistoryUpdated()
        {
            if (_pendingPage != null)
            {
                _historyPages[_webView.CopyBackForwardList().CurrentIndex] = _pendingPage;
                _pendingPage = null;
            }
            UpdateBackCallback();
        }

        internal void UpdateBackCallback()
        {
            if (_backCallback == null || !OperatingSystem.IsAndroidVersionAtLeast(33))
            {
                return;
            }
            var canGoBack = _webView.CanGoBack();

            if (canGoBack == _backCallbackRegistered)
            {
                return;
            }
            if (canGoBack)
            {
                OnBackInvokedDispatcher.RegisterOnBackInvokedCallback(IOnBackInvokedDispatcher.PriorityDefault, _backCallback);
            }
            else
            {
                OnBackInvokedDispatcher.UnregisterOnBackInvokedCallback(_backCallback);
            }
            _backCallbackRegistered = canGoBack;
        }

        private sealed class BackCallback : Java.Lang.Object, IOnBackInvokedCallback
        {
            private readonly Action _onBack;

            public BackCallback(Action onBack) => _onBack = onBack;

            public void OnBackInvoked() => _onBack();
        }

        private sealed class SystemBarsInsetsListener : Java.Lang.Object, View.IOnApplyWindowInsetsListener
        {
            public WindowInsets OnApplyWindowInsets(View v, WindowInsets insets)
            {
                var bars = insets.GetInsets(WindowInsets.Type.SystemBars() | WindowInsets.Type.DisplayCutout());

                v.SetPadding(bars.Left, bars.Top, bars.Right, bars.Bottom);

                return WindowInsets.Consumed;
            }
        }

        /// <summary>
        /// The views fire-and-forget $.ajax('hybrid:...') requests. Modern WebView refuses
        /// XHR to unknown schemes before any WebViewClient callback runs, so the pages get a
        /// small shim (see <see cref="HybridWebViewClient.InjectBridge"/>) that routes those
        /// requests to this JavaScript interface instead.
        /// </summary>
        private sealed class HybridBridge : Java.Lang.Object
        {
            public const string Name = "ChordsHost";

            private readonly HybridWebViewClient _client;
            private readonly WebView _webView;

            public HybridBridge(HybridWebViewClient client, WebView webView)
            {
                _client = client;
                _webView = webView;
            }

            [JavascriptInterface]
            [Export("invoke")]
            public void Invoke(string url)
            {
                // Called on the WebView's JavaBridge thread.
                if (url != null && url.StartsWith(HybridScheme))
                {
                    _client.HandleInBackground(_webView, url);
                }
            }
        }

        private class HybridWebViewClient : WebViewClient
        {
            private const string BridgeScript =
                "<script>(function(){" +
                "var p=XMLHttpRequest.prototype,o=p.open,s=p.send,h=p.setRequestHeader;" +
                "p.open=function(m,u){this.__hybrid=(typeof u==='string'&&u.indexOf('" + HybridScheme + "')===0)?u:null;" +
                "if(!this.__hybrid)return o.apply(this,arguments);};" +
                "p.setRequestHeader=function(){if(!this.__hybrid)return h.apply(this,arguments);};" +
                "p.send=function(){if(this.__hybrid){" + HybridBridge.Name + ".invoke(this.__hybrid);return;}return s.apply(this,arguments);};" +
                "})();</script>";

            private readonly MainActivity _activity;
            private readonly object _sync = new object();
			private Note _currentRoot;
			private ChordType _currentChordType;
            private bool _allowPartial;
            private SettingsModel _currentSettings = SettingsModel.Load();
            private Note[] _allRoots;
            private string[] _allChordTypes;

            public HybridWebViewClient(MainActivity activity)
            {
                _activity = activity;
            }

            public override void DoUpdateVisitedHistory(WebView view, string url, bool isReload)
            {
                base.DoUpdateVisitedHistory(view, url, isReload);
                _activity.OnHistoryUpdated();
            }

            public override bool ShouldOverrideUrlLoading(WebView webView, IWebResourceRequest request)
                => ShouldOverrideUrlLoading(webView, request.Url?.ToString());

            public override bool ShouldOverrideUrlLoading(WebView webView, string url)
            {
                // If the URL is not our own custom scheme, just let the webView load the URL as usual
                if (url == null || !url.StartsWith(HybridScheme))
                {
                    return false;
                }
                HandleInBackground(webView, url);

                return true;
            }

            /// <summary>
            /// Renders pages off the UI thread (chord layout generation and chord playback can
            /// take a while); every WebView call is posted back to the UI thread.
            /// </summary>
            public void HandleInBackground(WebView webView, string url)
            {
                Task.Run(() =>
                {
                    try
                    {
                        var method = url.Substring(HybridScheme.Length).Split('?')[0];

                        if (method == "PlayChord")
                        {
                            // Playback only touches the SoundPool; don't block navigation while it plays.
                            HandleUrl(webView, url);
                        }
                        else
                        {
                            lock (_sync)
                            {
                                HandleUrl(webView, url);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error handling {url}\n{e}");
                    }
                });
            }

            private bool HandleUrl(WebView webView, string url)
            {
                // This handler will treat everything between the protocol and "?"
                // as the method name.  The querystring has all of the parameters.
                var resources = url.Substring(HybridScheme.Length).Split('?');
                var method = resources[0];
                var parameters = resources.Length > 1 ? System.Web.HttpUtility.ParseQueryString(resources[1]) : new NameValueCollection();

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

            private void LoadPage(WebView webView, string page, string pageName)
            {
                page = InjectBridge(page);
                webView.Post(() =>
                {
                    webView.ScrollbarFadingEnabled = true;  //default value unless changed
                    _activity.OnPageLoading(pageName);
                    // Load the rendered HTML into the view with a base URL
                    // that points to the root of the bundled Assets folder
                    webView.LoadDataWithBaseURL($"file:///android_asset/?page={pageName}", page, "text/html", "UTF-8", null);
                });
            }

            private static void RunScript(WebView webView, string js)
            {
                webView.Post(() => webView.EvaluateJavascript(js, null));
            }

            internal static string InjectBridge(string page)
            {
                var head = page.IndexOf("<head>", StringComparison.OrdinalIgnoreCase);

                return head < 0
                    ? BridgeScript + page
                    : page.Insert(head + "<head>".Length, BridgeScript);
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
                LoadPage(webView, page, "ShowChord");
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

				LoadPage(webView, page, "ShowChordResults");
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
				LoadPage(webView, page, "ShowChordLayouts");
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

				LoadPage(webView, page, "FindChord");
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

                LoadPage(webView, page, "CircleOfFifths");
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
					RunScript(webView, js);
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
                    RunScript(webView, js);
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

				LoadPage(webView, page, "FavoriteChords");
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

				LoadPage(webView, page, "Settings");
			}
		}
    }
}
