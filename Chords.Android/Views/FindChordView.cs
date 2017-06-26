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

#line 2 "FindChordView.cshtml"
using System.Text.RegularExpressions;

#line default
#line hidden

#line 3 "FindChordView.cshtml"
using PortableRazor.Web.Mvc;

#line default
#line hidden


[System.CodeDom.Compiler.GeneratedCodeAttribute("RazorTemplatePreprocessor", "2.6.0.0")]
public partial class FindChordView : PortableRazor.ViewBase
{

#line hidden

#line 4 "FindChordView.cshtml"
public Chords.Android.Models.FindChordModel Model { get; set; }

#line default
#line hidden


public override void Execute()
{
WriteLiteral("﻿");

WriteLiteral("<!DOCTYPE html>\n<html>\n<head>\n    <link");

WriteLiteral(" rel=\"stylesheet\"");

WriteLiteral(" href=\"style.css\"");

WriteLiteral(" />\n    <link");

WriteLiteral(" rel=\"stylesheet\"");

WriteLiteral(" href=\"bootstrap.min.css\"");

WriteLiteral(" />\n\t<link");

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

WriteLiteral(">Find Chord</span>\n            </div>\n                \n            <div");

WriteLiteral(" class=\"navbar-collapse collapse\"");

WriteLiteral(">\n                <ul");

WriteLiteral(" class=\"nav navbar-nav\"");

WriteLiteral(">\n                    <li><a");

WriteLiteral(" href=\"hybrid:ShowChord\"");

WriteLiteral(">Show chord</a></li>\n                    <li><a");

WriteLiteral(" href=\"hybrid:FindChord\"");

WriteLiteral(">Find chord</a></li>\n                </ul>\n            </div>\n        </div>\n    " +
"</nav>\n    <script");

WriteLiteral(" src=\"jquery.min.js\"");

WriteLiteral("></script>\n    <script");

WriteLiteral(" src=\"bootstrap.min.js\"");

WriteLiteral("></script>\n\t<script");

WriteLiteral(" src=\"bootstrap-toggle.min.js\"");

WriteLiteral("></script>\n    <div");

WriteLiteral(" class=\"col-xs-12 col-sm-12 col-md-12 col-lg-12 main-content\"");

WriteLiteral(">\n\n\t\t<!-- Main content start -->\n        <nav");

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


#line 48 "FindChordView.cshtml"
                                                           Write(Model.conv);


#line default
#line hidden
WriteLiteral("</span>\n                </div>\n                    \n                <div");

WriteLiteral(" class=\"navbar-conv-collapse collapse\"");

WriteLiteral(">\n                    <ul");

WriteLiteral(" class=\"nav navbar-nav\"");

WriteLiteral(">\n");


#line 53 "FindChordView.cshtml"
                    

#line default
#line hidden

#line 53 "FindChordView.cshtml"
                     foreach(var conv in @Model.NamingConventions)
                    {


#line default
#line hidden
WriteLiteral("                        <li><a");

WriteAttribute ("href", " href=\"", "\""
, Tuple.Create<string,object,bool> ("", "hybrid:FindChord?conv=", true)

#line 55 "FindChordView.cshtml"
                    , Tuple.Create<string,object,bool> ("", conv

#line default
#line hidden
, false)
);
WriteLiteral(">");


#line 55 "FindChordView.cshtml"
                                                             Write(conv);


#line default
#line hidden
WriteLiteral("</a></li>\n");


#line 56 "FindChordView.cshtml"
                    }


#line default
#line hidden
WriteLiteral("                    </ul>\n                </div>\n            </div>\n        </nav" +
">\n\t\t<form");

WriteLiteral(" action=\"hybrid:FindChord\"");

WriteLiteral(">\n\t\t\t<input");

WriteLiteral(" type=\"hidden\"");

WriteLiteral(" name=\"conv\"");

WriteAttribute ("value", " value=\"", "\""

#line 62 "FindChordView.cshtml"
             , Tuple.Create<string,object,bool> ("", Model.conv

#line default
#line hidden
, false)
);
WriteLiteral(" />\n\t\t\t<div");

WriteLiteral(" class=\"text-center\"");

WriteLiteral(" style=\"margin-top:20px;\"");

WriteLiteral(">\n");


#line 64 "FindChordView.cshtml"
			

#line default
#line hidden

#line 64 "FindChordView.cshtml"
             for(var i = 0; i < Model.AllNotes[Model.conv].Length; i++)
			{
				var note = Regex.Replace(Model.AllNotes[Model.conv][i].ToString(), "<.*?>", string.Empty);
                if (i % 5 == 0)
                {
                    

#line default
#line hidden

#line 69 "FindChordView.cshtml"
                Write(new HtmlString("\t\t\t\t\t<div style=\"padding-bottom:5px;\">\n"));


#line default
#line hidden

#line 69 "FindChordView.cshtml"
                                                                                        
                }
				if (Model.SelectedNotes != null && Model.SelectedNotes.Contains(note))
				{


#line default
#line hidden
WriteLiteral("                    <input");

WriteLiteral(" type=\"checkbox\"");

WriteLiteral(" name=\"note\"");

WriteAttribute ("value", " value=\"", "\""

#line 73 "FindChordView.cshtml"
                       , Tuple.Create<string,object,bool> ("", note

#line default
#line hidden
, false)
);
WriteLiteral(" checked=\"checked\"");

WriteLiteral(" data-toggle=\"toggle\"");

WriteLiteral(" data-on=\"");


#line 73 "FindChordView.cshtml"
                                                                                                                 Write(Model.AllNotes[Model.conv][i]);


#line default
#line hidden
WriteLiteral("\"");

WriteLiteral(" data-off=\"");


#line 73 "FindChordView.cshtml"
                                                                                                                                                             Write(Model.AllNotes[Model.conv][i]);


#line default
#line hidden
WriteLiteral("\"");

WriteLiteral(" data-width=\"100px;\"");

WriteLiteral(" style=\"display:none;\"");

WriteLiteral(" />\n");


#line 74 "FindChordView.cshtml"
				}
				else
				{


#line default
#line hidden
WriteLiteral("                    <input");

WriteLiteral(" type=\"checkbox\"");

WriteLiteral(" name=\"note\"");

WriteAttribute ("value", " value=\"", "\""

#line 77 "FindChordView.cshtml"
                       , Tuple.Create<string,object,bool> ("", note

#line default
#line hidden
, false)
);
WriteLiteral(" data-toggle=\"toggle\"");

WriteLiteral(" data-on=\"");


#line 77 "FindChordView.cshtml"
                                                                                               Write(Model.AllNotes[Model.conv][i]);


#line default
#line hidden
WriteLiteral("\"");

WriteLiteral(" data-off=\"");


#line 77 "FindChordView.cshtml"
                                                                                                                                           Write(Model.AllNotes[Model.conv][i]);


#line default
#line hidden
WriteLiteral("\"");

WriteLiteral(" data-width=\"100px;\"");

WriteLiteral(" style=\"display:none;\"");

WriteLiteral(" />\n");


#line 78 "FindChordView.cshtml"
				}


#line default
#line hidden
WriteLiteral("                <span></span>\n");


#line 80 "FindChordView.cshtml"
				if (i % 5 == 4)
				{					
				    

#line default
#line hidden

#line 82 "FindChordView.cshtml"
                Write(new HtmlString("\t\t\t\t\t</div>\n"));


#line default
#line hidden

#line 82 "FindChordView.cshtml"
                                                           
				}
			}


#line default
#line hidden
WriteLiteral("            </div>\n\t\t\t<div");

WriteLiteral(" style=\"margin-top:20px;\"");

WriteLiteral(">\n\t\t\t\t<button");

WriteLiteral(" type=\"button\"");

WriteLiteral(" class=\"btn btn-default btn-lg\"");

WriteLiteral(" style=\"width:100%; display: none;\"");

WriteLiteral(" onclick=\"$(\'form\').submit();\"");

WriteLiteral(">Find</button>\n\t\t\t</div>\n\t\t</form>\n\t\t<div");

WriteLiteral(" class=\"error\"");

WriteLiteral(">");


#line 90 "FindChordView.cshtml"
                      Write(Model.Error);


#line default
#line hidden
WriteLiteral("</div>\n        <script");

WriteLiteral(" type=\"text/javascript\"");

WriteLiteral(@">				
            function updateNamingConvention(value) {
                location = 'hybrid:FindChord?conv=' + value;
            }
			$(document).ready(function(){
				$(""input,button"").show();
			});
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