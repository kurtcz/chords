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

namespace Chords.Android
{
    [Activity(Label = "Chords", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {
        private static Bundle _bundle;
        private WebView _webView;
		private static SoundPool _soundPool;
        private static int[] _soundIds;
        private static float[] _pitchRates =
        {
            1,
            16/15f,
            9/8f,
            6/5f,
            5/4f,
            4/3f,
            45/32f,
            3/2f,
            8/5f,
            5/3f,
            16/9f,
            15/8f
        };
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

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

			_webView = FindViewById<WebView>(Resource.Id.webView);
            _webView.Settings.JavaScriptEnabled = true;

            // Use subclassed WebViewClient to intercept hybrid native calls
            var viewClient = new HybridWebViewClient();
            _webView.SetWebViewClient(viewClient);

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
					PlayChord(parameters);
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
				var showChordParams = new ChordParams(parameters);
                var model = new ShowChordModel { conv = showChordParams.NamingConvention };
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
                    model.Layouts = model.ChordDecorator.GenerateLayouts(showChordParams.Special, showChordParams.Partial);
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

            public void PlayChord(NameValueCollection parameters)
            {
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
							var playRate = (halfSteps / _pitchRates.Length + 1) * _pitchRates[halfSteps % _pitchRates.Length];

							_soundPool.Play(_soundIds[i], 1, 1, 0, 0, playRate);
                            if (n == 0)
                            {
								Thread.Sleep(300);
							}
                            else
                            {
								Thread.Sleep(100);
							}
						}
					}
                    Thread.Sleep(300);
				}
			}

			public void UpdateLabel(WebView webView, NameValueCollection parameters)
			{
				var textbox = parameters["textbox"];

				// Add some text to our string here so that we know something
				// happened on the native part of the round trip.
				var prepended = $"C# says: {textbox}";

				// Build some javascript using the C#-modified result
				var js = $"SetLabelText('{prepended}');";

				webView.LoadUrl("javascript:" + js);
			}
		}
    }

    public abstract partial class ShowChordResultViewBase
    {
        
    }
}
