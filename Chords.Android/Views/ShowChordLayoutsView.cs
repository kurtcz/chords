#pragma warning disable 1591
// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      Mono Runtime Version: 4.0.30319.42000
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------

namespace Chords.Android.Views
{
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#line 1 "ShowChordLayoutsView.cshtml"
using Chords.Core.Extensions;

#line default
#line hidden


[System.CodeDom.Compiler.GeneratedCodeAttribute("RazorTemplatePreprocessor", "2.6.0.0")]
public partial class ShowChordLayoutsView : PortableRazor.ViewBase
{

#line hidden

#line 3 "ShowChordLayoutsView.cshtml"
public Chords.Android.Models.ShowChordLayoutsModel Model { get; set; }

#line default
#line hidden


public override void Execute()
{

#line 4 "ShowChordLayoutsView.cshtml"
  
    var otherSymbols = Model.ChordDecorator != null ? Model.ChordDecorator.Symbols.Skip(1) : new string[0];
    //onclick="playChord('@positions');" 


#line default
#line hidden
WriteLiteral("\n<!DOCTYPE html>\n<html>\n<head>\n\t<!--link rel=\"stylesheet\" href=\"jquery.mobile-1.4" +
".5.min.css\" /-->\n    <link");

WriteLiteral(" rel=\"stylesheet\"");

WriteLiteral(" href=\"style.css\"");

WriteLiteral(" />\n    <link");

WriteLiteral(" rel=\"stylesheet\"");

WriteLiteral(" href=\"bootstrap.min.css\"");

WriteLiteral(" />\n</head>\n<body>\n    <nav");

WriteLiteral(" class=\"navbar navbar-inverse navbar-fixed-top\"");

WriteLiteral(">\n        <div");

WriteLiteral(" class=\"container\"");

WriteLiteral(">\n            <div");

WriteLiteral(" class=\"navbar-header\"");

WriteLiteral(">\n                <button");

WriteLiteral(" type=\"button\"");

WriteLiteral(" class=\"navbar-toggle\"");

WriteLiteral(" data-toggle=\"collapse\"");

WriteLiteral(" data-target=\".navbar-collapse\"");

WriteLiteral(">\n                    <span");

WriteLiteral(" class=\"sr-only\"");

WriteLiteral(">Toggle navigation</span>\n                    <span");

WriteLiteral(" class=\"icon-bar\"");

WriteLiteral("></span>\n                    <span");

WriteLiteral(" class=\"icon-bar\"");

WriteLiteral("></span>\n                    <span");

WriteLiteral(" class=\"icon-bar\"");

WriteLiteral("></span>\n                </button>\n                <span");

WriteLiteral(" class=\"navbar-brand\"");

WriteLiteral(">Show Chord</span>\n            </div>\n                \n            <div");

WriteLiteral(" class=\"navbar-collapse collapse\"");

WriteLiteral(">\n                <ul");

WriteLiteral(" class=\"nav navbar-nav\"");

WriteLiteral(">\n                    <li><a");

WriteAttribute ("href", " href=\"", "\""
, Tuple.Create<string,object,bool> ("", "hybrid:ShowChord?conv=", true)

#line 30 "ShowChordLayoutsView.cshtml"
                , Tuple.Create<string,object,bool> ("", Model.Parameters["conv"]

#line default
#line hidden
, false)
);
WriteLiteral(">Show chord</a></li>\n                    <li><a");

WriteAttribute ("href", " href=\"", "\""
, Tuple.Create<string,object,bool> ("", "hybrid:FindChord?conv=", true)

#line 31 "ShowChordLayoutsView.cshtml"
                , Tuple.Create<string,object,bool> ("", Model.Parameters["conv"]

#line default
#line hidden
, false)
);
WriteLiteral(">Find chord</a></li>\n\t\t\t\t\t<li><a");

WriteAttribute ("href", " href=\"", "\""
, Tuple.Create<string,object,bool> ("", "hybrid:Circle?conv=", true)

#line 32 "ShowChordLayoutsView.cshtml"
             , Tuple.Create<string,object,bool> ("", Model.Parameters["conv"]

#line default
#line hidden
, false)
);
WriteLiteral(">Circle of fifths</a></li>\n\t\t\t\t\t<li><a");

WriteAttribute ("href", " href=\"", "\""
, Tuple.Create<string,object,bool> ("", "hybrid:FavoriteChords?conv=", true)

#line 33 "ShowChordLayoutsView.cshtml"
                     , Tuple.Create<string,object,bool> ("", Model.Parameters["conv"]

#line default
#line hidden
, false)
);
WriteLiteral(">Favourite chords</a></li>\n                </ul>\n            </div>\n        </div" +
">\n    </nav>\n    <script");

WriteLiteral(" src=\"jquery.min.js\"");

WriteLiteral("></script>\n\t<script");

WriteLiteral(" src=\"jquery.mobile-1.4.5.min.js\"");

WriteLiteral("></script>\n    <script");

WriteLiteral(" src=\"bootstrap.min.js\"");

WriteLiteral("></script>\n    <div");

WriteLiteral(" class=\"col-xs-12 col-sm-12 col-md-12 col-lg-12 main-content\"");

WriteLiteral(">\n\n        <!-- Main content start -->\n");


#line 44 "ShowChordLayoutsView.cshtml"
		

#line default
#line hidden

#line 44 "ShowChordLayoutsView.cshtml"
         if (Model.ChordDecorator != null)
	    {


#line default
#line hidden
WriteLiteral("\t        <h2>");


#line 46 "ShowChordLayoutsView.cshtml"
           Write(Model.ChordDecorator.Symbols[0]);


#line default
#line hidden
WriteLiteral("</h2>\n");


#line 47 "ShowChordLayoutsView.cshtml"

	        if (otherSymbols.Any())
	        {


#line default
#line hidden
WriteLiteral("\t            <h4>Other symbols:</h4>\n");


#line 51 "ShowChordLayoutsView.cshtml"
	            foreach(var otherSymbol in otherSymbols)
	            {


#line default
#line hidden
WriteLiteral("\t                <span>");


#line 53 "ShowChordLayoutsView.cshtml"
                     Write(otherSymbol);


#line default
#line hidden
WriteLiteral("</span>\n");


#line 54 "ShowChordLayoutsView.cshtml"
	            }
	        }


#line default
#line hidden
WriteLiteral("\t        <h4>Chord intervals:</h4>\n");

WriteLiteral("\t        <p>\n");


#line 58 "ShowChordLayoutsView.cshtml"
	        

#line default
#line hidden

#line 58 "ShowChordLayoutsView.cshtml"
             foreach(var interval in Model.ChordDecorator.Intervals)
	        {


#line default
#line hidden
WriteLiteral("\t            <span>");


#line 60 "ShowChordLayoutsView.cshtml"
                 Write(interval);


#line default
#line hidden
WriteLiteral("</span>\n");


#line 61 "ShowChordLayoutsView.cshtml"
	        }


#line default
#line hidden
WriteLiteral("\t        </p>\n");


#line 63 "ShowChordLayoutsView.cshtml"



#line default
#line hidden
WriteLiteral("\t        <h4>Chord notes:</h4>\n");

WriteLiteral("\t        <p>\n");


#line 66 "ShowChordLayoutsView.cshtml"
	        

#line default
#line hidden

#line 66 "ShowChordLayoutsView.cshtml"
             foreach(var note in Model.ChordDecorator.Notes)
	        {


#line default
#line hidden
WriteLiteral("\t            <span>");


#line 68 "ShowChordLayoutsView.cshtml"
                 Write(note);


#line default
#line hidden
WriteLiteral("</span>\n");


#line 69 "ShowChordLayoutsView.cshtml"
	        }


#line default
#line hidden
WriteLiteral("\t        </p>\n");

WriteLiteral("\t        <div");

WriteLiteral(" id=\"error\"");

WriteLiteral(" class=\"error\"");

WriteLiteral(">");


#line 71 "ShowChordLayoutsView.cshtml"
                                     Write(Model.Error);


#line default
#line hidden
WriteLiteral("</div>\n");

WriteLiteral("\t        <div");

WriteLiteral(" id=\"loader\"");

WriteLiteral("></div>\n");

WriteLiteral("\t        <div");

WriteLiteral(" id=\"layouts\"");

WriteLiteral(" class=\"animate-bottom\"");

WriteLiteral(">\n");


#line 74 "ShowChordLayoutsView.cshtml"
	        

#line default
#line hidden

#line 74 "ShowChordLayoutsView.cshtml"
             if (Model.Layouts != null)
	        {


#line default
#line hidden
WriteLiteral("\t            <h4>Chord layouts:</h4>\n");

WriteLiteral("\t            <p>\n");

WriteLiteral("\t                ");


#line 78 "ShowChordLayoutsView.cshtml"
               Write(Model.Layouts.Length);


#line default
#line hidden
WriteLiteral(" results\n");


#line 79 "ShowChordLayoutsView.cshtml"
	                

#line default
#line hidden

#line 79 "ShowChordLayoutsView.cshtml"
                     foreach(var layout in Model.Layouts)
	                {
					    var positions = string.Join(",", layout.IntPositions.Select(i => i.ToString()).ToArray());
					


#line default
#line hidden
WriteLiteral("\t                    <div");

WriteLiteral(" class=\"col-xs-6 col-sm-4 col-md-3 col-lg-2 chord-layout\"");

WriteLiteral(" data-favorite=\"");


#line 83 "ShowChordLayoutsView.cshtml"
                                                                                                 Write(layout.Favorite);


#line default
#line hidden
WriteLiteral("\"");

WriteLiteral(" data-positions=\"");


#line 83 "ShowChordLayoutsView.cshtml"
                                                                                                                                    Write(positions);


#line default
#line hidden
WriteLiteral("\"");

WriteAttribute ("onclick", " onclick=\"", "\""
, Tuple.Create<string,object,bool> ("", "playChord(\'", true)

#line 83 "ShowChordLayoutsView.cshtml"
                                                                                                                                  , Tuple.Create<string,object,bool> ("", positions

#line default
#line hidden
, false)
, Tuple.Create<string,object,bool> ("", "\');", true)
);
WriteLiteral(">\n");

WriteLiteral("\t                        ");


#line 84 "ShowChordLayoutsView.cshtml"
                        Write(layout.Schema);


#line default
#line hidden
WriteLiteral("\n\t                    </div>\n");


#line 86 "ShowChordLayoutsView.cshtml"
	                }


#line default
#line hidden
WriteLiteral("\t            </p>\n");

WriteLiteral("\t            <script");

WriteLiteral(" type=\"text/javascript\"");

WriteLiteral(@">
	                $('#loader').hide();
	                $('#layouts').show();
					function playChord(positions) {
	                    $.ajax({
	                        url: 'hybrid:PlayChord',
	                        type: 'get',
	                        data: {
	                            positions: positions
	                        }
	                    });
					}
					function setFavorite(selector, value) {
					    $(selector).attr('data-favorite', value);
					}
					$(function(){
					    $('.ui-loader').hide();
					    $.event.special.tap.tapholdThreshold = 300;
					    $.event.special.tap.emitTapOnTaphold = false;
					    $('.chord-layout').bind('taphold', function(){
                            var positions = $(this).attr('data-positions');
					        var favorite = $(this).attr('data-favorite') == ""True"";

					        $.ajax({
	                            url: favorite ? 'hybrid:RemoveFromFavorites' : 'hybrid:AddToFavorites',
	                            type: 'get',
	                            data: {
					                root: '");


#line 115 "ShowChordLayoutsView.cshtml"
                                      Write(Model.ChordDecorator.Root);


#line default
#line hidden
WriteLiteral("\',\n\t\t\t\t\t                type: \'");


#line 116 "ShowChordLayoutsView.cshtml"
                                      Write(Model.ChordDecorator.ChordType.ToDescription());


#line default
#line hidden
WriteLiteral("\',\n\t\t\t\t\t                conv: \'");


#line 117 "ShowChordLayoutsView.cshtml"
                                      Write(Model.Parameters["conv"]);


#line default
#line hidden
WriteLiteral("\',\n\t\t\t\t\t                positions: positions\n\t                            }\n\t    " +
"                    });\n\t\t\t\t\t    });\n\t\t\t\t\t});\n            </script>\n");


#line 124 "ShowChordLayoutsView.cshtml"
	        }


#line default
#line hidden
WriteLiteral("\t\t\t</div>\n");


#line 126 "ShowChordLayoutsView.cshtml"
		}


#line default
#line hidden
WriteLiteral("        <!-- Main content end -->   \n\n    </div>\n    <hr />\n    <footer>\n        " +
"<p>&copy; 2017 - Tomáš Němec</p>\n    </footer>\n</body>\n</html>\n");

}
}
}
#pragma warning restore 1591
