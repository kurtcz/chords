﻿using System;
using Android.App;
using Android.Content;
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

namespace Chords.Android
{
    [Activity(Label = "Guitar Chords", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            var webView = FindViewById<WebView>(Resource.Id.webView);
            webView.Settings.JavaScriptEnabled = true;

            // Use subclassed WebViewClient to intercept hybrid native calls
            var viewClient = new HybridWebViewClient();
            webView.SetWebViewClient(viewClient);

            viewClient.ShowChord(webView, new NameValueCollection());
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
						resultModel.Error = $"{id} is not a valid chord";
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
                    var model = new FindChordModel { conv = chordParams.NamingConvention };
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
                    var chord = Chord.Find(notes);

                    if (chord != null)
                    {
                        parameters.Add("root", chord.Root.ToString(chordParams.NamingConvention));
                        parameters.Add("type", chord.ChordType.ToDescription());
                        parameters.Add("partial", "true");
                        ShowChord(webView, parameters);
                        return;
                    }
                    var model = new FindChordModel { conv = chordParams.NamingConvention, Error = "No chord found" };
					var template = new FindChordView() { Model = model };

					page = template.GenerateString();
				}

				// Load the rendered HTML into the view with a base URL 
				// that points to the root of the bundled Assets folder
				webView.LoadDataWithBaseURL("file:///android_asset/", page, "text/html", "UTF-8", null);
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
