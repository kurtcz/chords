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

#line 2 "ShowChordView.cshtml"
using Chords.Core.Extensions;

#line default
#line hidden


[System.CodeDom.Compiler.GeneratedCodeAttribute("RazorTemplatePreprocessor", "2.6.0.0")]
public partial class ShowChordView : PortableRazor.ViewBase
{

#line hidden

#line 3 "ShowChordView.cshtml"
public Chords.Android.Models.ShowChordModel Model { get; set; }

#line default
#line hidden


public override void Execute()
{
WriteLiteral("<!DOCTYPE html>\n<html>\n<head>\n    <link");

WriteLiteral(" rel=\"stylesheet\"");

WriteLiteral(" href=\"style.css\"");

WriteLiteral(" />\n\t<link");

WriteLiteral(" rel=\"stylesheet\"");

WriteLiteral(" href=\"bootstrap.min.css\"");

WriteLiteral(" />\n    <link");

WriteLiteral(" rel=\"stylesheet\"");

WriteLiteral(" href=\"bootstrap-toggle.min.css\"");

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

WriteLiteral(">Show chord chart</span>\n            </div>\n                \n            <div");

WriteLiteral(" class=\"navbar-collapse collapse\"");

WriteLiteral(">\n                <ul");

WriteLiteral(" class=\"nav navbar-nav\"");

WriteLiteral(">\n                    <li><a");

WriteAttribute ("href", " href=\"", "\""
, Tuple.Create<string,object,bool> ("", "hybrid:ShowChord?conv=", true)

#line 26 "ShowChordView.cshtml"
                , Tuple.Create<string,object,bool> ("", Model.conv

#line default
#line hidden
, false)
);
WriteLiteral(">Show chord chart</a></li>\n                    <li><a");

WriteAttribute ("href", " href=\"", "\""
, Tuple.Create<string,object,bool> ("", "hybrid:FindChord?conv=", true)

#line 27 "ShowChordView.cshtml"
                , Tuple.Create<string,object,bool> ("", Model.conv

#line default
#line hidden
, false)
);
WriteLiteral(">Find chord name</a></li>\n\t\t\t\t\t<li><a");

WriteAttribute ("href", " href=\"", "\""
, Tuple.Create<string,object,bool> ("", "hybrid:Circle?conv=", true)

#line 28 "ShowChordView.cshtml"
             , Tuple.Create<string,object,bool> ("", Model.conv

#line default
#line hidden
, false)
);
WriteLiteral(">Circle of fifths</a></li>\n\t\t\t\t\t<li><a");

WriteAttribute ("href", " href=\"", "\""
, Tuple.Create<string,object,bool> ("", "hybrid:FavoriteChords?conv=", true)

#line 29 "ShowChordView.cshtml"
                     , Tuple.Create<string,object,bool> ("", Model.conv

#line default
#line hidden
, false)
);
WriteLiteral(">Favourite chords</a></li>\n                </ul>\n            </div>\n        </div" +
">\n    </nav>\n    <script");

WriteLiteral(" src=\"jquery.min.js\"");

WriteLiteral("></script>\n\t<script");

WriteLiteral(" src=\"bootstrap.min.js\"");

WriteLiteral("></script>\n\t<script");

WriteLiteral(" src=\"bootstrap-toggle.min.js\"");

WriteLiteral("></script>\n    <div");

WriteLiteral(" class=\"col-xs-12 col-sm-12 col-md-12 col-lg-12 main-content\"");

WriteLiteral(">\n\n\t\t<!-- Main content start -->\n\t    <nav");

WriteLiteral(" class=\"navbar navbar-inverse\"");

WriteLiteral(">\n\t        <div");

WriteLiteral(" class=\"container\"");

WriteLiteral(">\n\t            <div");

WriteLiteral(" class=\"navbar-header\"");

WriteLiteral(">\n\t                <button");

WriteLiteral(" type=\"button\"");

WriteLiteral(" class=\"navbar-toggle\"");

WriteLiteral(" data-toggle=\"collapse\"");

WriteLiteral(" data-target=\".navbar-root-collapse\"");

WriteLiteral(">\n\t                    <span");

WriteLiteral(" class=\"sr-only\"");

WriteLiteral(">Toggle root</span>\n\t                    <span");

WriteLiteral(" class=\"icon-bar\"");

WriteLiteral("></span>\n\t                    <span");

WriteLiteral(" class=\"icon-bar\"");

WriteLiteral("></span>\n\t                    <span");

WriteLiteral(" class=\"icon-bar\"");

WriteLiteral("></span>\n\t                </button>\n\t                <span");

WriteLiteral(" class=\"navbar-brand\"");

WriteLiteral(" id=\"navbar-root\"");

WriteLiteral(">");


#line 49 "ShowChordView.cshtml"
                                                            Write(Model.Root?.ToString(Model.conv) ?? Model.Notes[Model.conv][1]);


#line default
#line hidden
WriteLiteral("</span>\n\t            </div>\n\t                \n\t            <div");

WriteLiteral(" class=\"navbar-root-collapse collapse\"");

WriteLiteral(">\n\t                <ul");

WriteLiteral(" class=\"nav navbar-nav\"");

WriteLiteral(">\n");


#line 54 "ShowChordView.cshtml"
					

#line default
#line hidden

#line 54 "ShowChordView.cshtml"
                     foreach(var note in @Model.Notes[Model.conv])
					{


#line default
#line hidden
WriteLiteral("                        <li><a");

WriteAttribute ("onclick", " onclick=\"", "\""
, Tuple.Create<string,object,bool> ("", "updateRoot(\'", true)

#line 56 "ShowChordView.cshtml"
              , Tuple.Create<string,object,bool> ("", note

#line default
#line hidden
, false)
, Tuple.Create<string,object,bool> ("", "\');", true)
);
WriteLiteral(" data-toggle=\"collapse\"");

WriteLiteral(" data-target=\".navbar-root-collapse\"");

WriteLiteral(">");


#line 56 "ShowChordView.cshtml"
                                                                                                                      Write(note);


#line default
#line hidden
WriteLiteral("</a></li>\n");


#line 57 "ShowChordView.cshtml"
					}


#line default
#line hidden
WriteLiteral("\t                </ul>\n\t            </div>\n\t        </div>\n\t    </nav>\n        <n" +
"av");

WriteLiteral(" class=\"navbar navbar-inverse\"");

WriteLiteral(">\n            <div");

WriteLiteral(" class=\"container\"");

WriteLiteral(">\n                <div");

WriteLiteral(" class=\"navbar-header\"");

WriteLiteral(">\n                    <button");

WriteLiteral(" type=\"button\"");

WriteLiteral(" class=\"navbar-toggle\"");

WriteLiteral(" data-toggle=\"collapse\"");

WriteLiteral(" data-target=\".navbar-type-collapse\"");

WriteLiteral(">\n                        <span");

WriteLiteral(" class=\"sr-only\"");

WriteLiteral(">Toggle chord type</span>\n                        <span");

WriteLiteral(" class=\"icon-bar\"");

WriteLiteral("></span>\n                        <span");

WriteLiteral(" class=\"icon-bar\"");

WriteLiteral("></span>\n                        <span");

WriteLiteral(" class=\"icon-bar\"");

WriteLiteral("></span>\n                    </button>\n                    <span");

WriteLiteral(" class=\"navbar-brand\"");

WriteLiteral(" id=\"navbar-type\"");

WriteLiteral(">");


#line 71 "ShowChordView.cshtml"
                                                            Write(Model.ChordType.ToDescription() ?? "Major");


#line default
#line hidden
WriteLiteral("</span>\n                </div>\n                    \n                <div");

WriteLiteral(" class=\"navbar-type-collapse collapse\"");

WriteLiteral(">\n                    <ul");

WriteLiteral(" class=\"nav navbar-nav\"");

WriteLiteral(">\n");


#line 76 "ShowChordView.cshtml"
                    

#line default
#line hidden

#line 76 "ShowChordView.cshtml"
                     foreach(var type in @Model.ChordTypeList)
                    {


#line default
#line hidden
WriteLiteral("                        <li><a");

WriteAttribute ("onclick", " onclick=\"", "\""
, Tuple.Create<string,object,bool> ("", "updateChordType(\'", true)

#line 78 "ShowChordView.cshtml"
                  , Tuple.Create<string,object,bool> ("", type.Key

#line default
#line hidden
, false)
, Tuple.Create<string,object,bool> ("", "\',", true)
, Tuple.Create<string,object,bool> (" ", "\'", true)

#line 78 "ShowChordView.cshtml"
                               , Tuple.Create<string,object,bool> ("", type.Value

#line default
#line hidden
, false)
, Tuple.Create<string,object,bool> ("", "\');", true)
);
WriteLiteral(" data-toggle=\"collapse\"");

WriteLiteral(" data-target=\".navbar-type-collapse\"");

WriteLiteral(">");


#line 78 "ShowChordView.cshtml"
                                                                                                                                             Write(type.Key);


#line default
#line hidden
WriteLiteral("</a></li>\n");


#line 79 "ShowChordView.cshtml"
                    }


#line default
#line hidden
WriteLiteral("                    </ul>\n                </div>\n            </div>\n        </nav" +
">\n        <nav");

WriteLiteral(" class=\"navbar navbar-inverse\"");

WriteLiteral(">\n            <div");

WriteLiteral(" class=\"container\"");

WriteLiteral(">\n                <div");

WriteLiteral(" class=\"navbar-header\"");

WriteLiteral(">\n                    <button");

WriteLiteral(" type=\"button\"");

WriteLiteral(" class=\"navbar-toggle\"");

WriteLiteral(" data-toggle=\"collapse\"");

WriteLiteral(" data-target=\".navbar-conv-collapse\"");

WriteLiteral(">\n                        <span");

WriteLiteral(" class=\"sr-only\"");

WriteLiteral(">Toggle naming convention</span>\n                        <span");

WriteLiteral(" class=\"icon-bar\"");

WriteLiteral("></span>\n                        <span");

WriteLiteral(" class=\"icon-bar\"");

WriteLiteral("></span>\n                        <span");

WriteLiteral(" class=\"icon-bar\"");

WriteLiteral("></span>\n                    </button>\n                    <span");

WriteLiteral(" class=\"navbar-brand\"");

WriteLiteral(" id=\"navbar-conv\"");

WriteLiteral(">");


#line 93 "ShowChordView.cshtml"
                                                            Write(Model.conv.ToDescription());


#line default
#line hidden
WriteLiteral("</span>\n                </div>\n                    \n                <div");

WriteLiteral(" class=\"navbar-conv-collapse collapse\"");

WriteLiteral(">\n                    <ul");

WriteLiteral(" class=\"nav navbar-nav\"");

WriteLiteral(">\n");


#line 98 "ShowChordView.cshtml"
                    

#line default
#line hidden

#line 98 "ShowChordView.cshtml"
                     foreach(var conv in @Model.NamingConventions)
                    {


#line default
#line hidden
WriteLiteral("                        <li><a");

WriteAttribute ("href", " href=\"", "\""
, Tuple.Create<string,object,bool> ("", "hybrid:ShowChord?conv=", true)

#line 100 "ShowChordView.cshtml"
                    , Tuple.Create<string,object,bool> ("", conv

#line default
#line hidden
, false)
);
WriteLiteral(">");


#line 100 "ShowChordView.cshtml"
                                                              Write(conv.ToDescription());


#line default
#line hidden
WriteLiteral("</a></li>\n");


#line 101 "ShowChordView.cshtml"
                    }


#line default
#line hidden
WriteLiteral("                    </ul>\n                </div>\n            </div>\n        </nav" +
">\n\t\t<form");

WriteLiteral(" action=\"hybrid:ShowChord\"");

WriteLiteral(">\n\t\t\t<input");

WriteLiteral(" type=\"hidden\"");

WriteLiteral(" name=\"root\"");

WriteLiteral(" id=\"root\"");

WriteAttribute ("value", " value=\"", "\""

#line 107 "ShowChordView.cshtml"
                        , Tuple.Create<string,object,bool> ("", Model.Root?.ToString(Model.conv) ?? Model.Notes[Model.conv][1]

#line default
#line hidden
, false)
);
WriteLiteral(" />\n            <input");

WriteLiteral(" type=\"hidden\"");

WriteLiteral(" name=\"type\"");

WriteLiteral(" id=\"type\"");

WriteAttribute ("value", " value=\"", "\""

#line 108 "ShowChordView.cshtml"
                        , Tuple.Create<string,object,bool> ("", Model.ChordType.ToDescription() ?? string.Empty

#line default
#line hidden
, false)
);
WriteLiteral(" />\n\t\t\t<input");

WriteLiteral(" type=\"hidden\"");

WriteLiteral(" name=\"conv\"");

WriteLiteral(" id=\"conv\"");

WriteAttribute ("value", " value=\"", "\""

#line 109 "ShowChordView.cshtml"
                       , Tuple.Create<string,object,bool> ("", Model.conv

#line default
#line hidden
, false)
);
WriteLiteral(" />\n\t\t\t<div");

WriteLiteral(" style=\"margin-bottom: 20px\"");

WriteLiteral(">\n\t\t\t\t<span");

WriteLiteral(" class=\"col-xs-9 col-sm-9 col-md-9 col-lg-9\"");

WriteLiteral(">Allow partial chords</span>\n\t            <input");

WriteLiteral(" type=\"checkbox\"");

WriteLiteral(" name=\"partial\"");

WriteLiteral(" value=\"true\"");

WriteLiteral(" data-toggle=\"toggle\"");

WriteLiteral(" data-on=\"Yes\"");

WriteLiteral(" data-off=\"No\"");

WriteLiteral(" checked=\"checked\"");

WriteLiteral(" />\n            </div>\n\t\t\t<div");

WriteLiteral(" style=\"margin-bottom: 20px\"");

WriteLiteral(">\n\t\t\t\t<span");

WriteLiteral(" class=\"col-xs-9 col-sm-9 col-md-9 col-lg-9\"");

WriteLiteral(">Allow special chords</span>\n\t            <input");

WriteLiteral(" type=\"checkbox\"");

WriteLiteral(" name=\"special\"");

WriteLiteral(" value=\"true\"");

WriteLiteral(" data-toggle=\"toggle\"");

WriteLiteral(" data-on=\"Yes\"");

WriteLiteral(" data-off=\"No\"");

WriteLiteral(" />\n            </div>\n\t\t\t<button");

WriteLiteral(" type=\"button\"");

WriteLiteral(" class=\"btn btn-default btn-lg\"");

WriteLiteral(" style=\"width:100%\"");

WriteLiteral(" onclick=\"$(\'form\').submit();\"");

WriteLiteral(">Find</button>\n\t\t</form>\n\t\t<div");

WriteLiteral(" class=\"error\"");

WriteLiteral("></div>\n\t\t<div");

WriteLiteral(" id=\"findresult\"");

WriteLiteral("></div>\n\t\t<script");

WriteLiteral(" type=\"text/javascript\"");

WriteLiteral(@">
			function updateRoot(value) {
				$('#navbar-root').html(value);
				$('#root').val(value);
			}
            function updateChordType(text, value) {
                $('#navbar-type').html(text);
                $('#type').val(value);
            }
		</script>
		<!-- Main content end -->
			
    </div>
    <hr />
    <footer>
        <p>&copy; 2017 - Tomáš Němec</p>
    </footer>
</body>
</html>
");

}
}
}
#pragma warning restore 1591
