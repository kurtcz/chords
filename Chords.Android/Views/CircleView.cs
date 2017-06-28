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

#line 2 "CircleView.cshtml"
using PortableRazor.Web.Mvc;

#line default
#line hidden

#line 3 "CircleView.cshtml"
using Chords.Core.Extensions;

#line default
#line hidden


[System.CodeDom.Compiler.GeneratedCodeAttribute("RazorTemplatePreprocessor", "2.6.0.0")]
public partial class CircleView : PortableRazor.ViewBase
{

#line hidden

#line 4 "CircleView.cshtml"
public Chords.Android.Models.CircleModel Model { get; set; }

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

WriteLiteral(">Circle of Fifths</span>\n            </div>\n                \n            <div");

WriteLiteral(" class=\"navbar-collapse collapse\"");

WriteLiteral(">\n                <ul");

WriteLiteral(" class=\"nav navbar-nav\"");

WriteLiteral(">\n                    <li><a");

WriteAttribute ("href", " href=\"", "\""
, Tuple.Create<string,object,bool> ("", "hybrid:ShowChord?conv=", true)

#line 27 "CircleView.cshtml"
                , Tuple.Create<string,object,bool> ("", Model.conv

#line default
#line hidden
, false)
);
WriteLiteral(">Show chord</a></li>\n                    <li><a");

WriteAttribute ("href", " href=\"", "\""
, Tuple.Create<string,object,bool> ("", "hybrid:FindChord?conv=", true)

#line 28 "CircleView.cshtml"
                , Tuple.Create<string,object,bool> ("", Model.conv

#line default
#line hidden
, false)
);
WriteLiteral(">Find chord</a></li>\n\t\t\t\t\t<li><a");

WriteAttribute ("href", " href=\"", "\""
, Tuple.Create<string,object,bool> ("", "hybrid:Circle?conv=", true)

#line 29 "CircleView.cshtml"
             , Tuple.Create<string,object,bool> ("", Model.conv

#line default
#line hidden
, false)
);
WriteLiteral(">Circle of Fifths</a></li>\n                </ul>\n            </div>\n        </div" +
">\n    </nav>\n    <script");

WriteLiteral(" src=\"jquery.min.js\"");

WriteLiteral("></script>\n    <script");

WriteLiteral(" src=\"bootstrap.min.js\"");

WriteLiteral("></script>\n    <script");

WriteLiteral(" src=\"bootstrap-toggle.min.js\"");

WriteLiteral("></script>\n    <div");

WriteLiteral(" class=\"col-xs-12 col-sm-12 col-md-12 col-lg-12 main-content\"");

WriteLiteral(">\n\n        <!-- Main content start -->\n        <nav");

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


#line 49 "CircleView.cshtml"
                                                            Write(Model.conv.ToDescription());


#line default
#line hidden
WriteLiteral("</span>\n                </div>\n                    \n                <div");

WriteLiteral(" class=\"navbar-conv-collapse collapse\"");

WriteLiteral(">\n                    <ul");

WriteLiteral(" class=\"nav navbar-nav\"");

WriteLiteral(">\n");


#line 54 "CircleView.cshtml"
                    

#line default
#line hidden

#line 54 "CircleView.cshtml"
                     foreach(var conv in @Model.NamingConventions)
                    {


#line default
#line hidden
WriteLiteral("                        <li><a");

WriteAttribute ("href", " href=\"", "\""
, Tuple.Create<string,object,bool> ("", "hybrid:Circle?conv=", true)

#line 56 "CircleView.cshtml"
                 , Tuple.Create<string,object,bool> ("", conv

#line default
#line hidden
, false)
);
WriteLiteral(">");


#line 56 "CircleView.cshtml"
                                                           Write(conv.ToDescription());


#line default
#line hidden
WriteLiteral("</a></li>\n");


#line 57 "CircleView.cshtml"
                    }


#line default
#line hidden
WriteLiteral("                    </ul>\n                </div>\n            </div>\n        </nav" +
">\n        <div");

WriteLiteral(" class=\"error\"");

WriteLiteral(">");


#line 62 "CircleView.cshtml"
                      Write(Model.Error);


#line default
#line hidden
WriteLiteral("</div>\n\t\t<div");

WriteLiteral(" style=\"text-align: center;\"");

WriteLiteral(">\n\t\t\t<svg");

WriteLiteral(" class=\"circle-svg\"");

WriteLiteral(" width=\"320\"");

WriteLiteral(" height=\"320\"");

WriteLiteral(">\n");


#line 65 "CircleView.cshtml"
				

#line default
#line hidden

#line 65 "CircleView.cshtml"
                 for(var i = 1; i <= 12; i++)
				{
					var idx = i <= 6 ? Model.index + i - 1 : Model.index + i - 1 - 12;
					if (idx < 0)
					{
					    idx += 12;
					}
					else if (idx >= Model.MajorCircle[Model.conv].Length)
					{
					    idx -= 12;
					}
					var id = Model.MajorCircle[Model.conv][idx];
					var color = i == 1 || i == 2  || i == 12 ? "#4d89d5" : "black";

                    

#line default
#line hidden

#line 79 "CircleView.cshtml"
                Write(new HtmlString($"\t\t\t\t<path id=\"arc-maj-{i}\" fill=\"none\" stroke=\"{color}\" onclick=\"location.href='hybrid:PlayChord?conv={Model.conv}&id={id}';\" />\n"));


#line default
#line hidden

#line 79 "CircleView.cshtml"
                                                                                                                                                                                        
                    

#line default
#line hidden

#line 80 "CircleView.cshtml"
                Write(new HtmlString($"\t\t\t\t<text id=\"text-maj-{i}\" text-anchor=\"middle\" stroke=\"none\" fill=\"white\" font-size=\"24\" onclick=\"location.href='hybrid:PlayChord?conv={Model.conv}&id={id}';\">{id}</text>\n"));


#line default
#line hidden

#line 80 "CircleView.cshtml"
                                                                                                                                                                                                                                        
				}


#line default
#line hidden
WriteLiteral("                ");


#line 82 "CircleView.cshtml"
                 for(var i = 1; i <= 12; i++)
                {
                    var idx = (Model.index + i - 1) % 12;
					var id = Model.MinorCircle[Model.conv][idx];
                    var color = i == 1 || i == 2 || i == 12 ? "#4d89d5" : "black";

					

#line default
#line hidden

#line 88 "CircleView.cshtml"
                Write(new HtmlString($"\t\t\t\t<path id=\"arc-min-{i}\" fill=\"none\" stroke=\"{color}\" onclick=\"location.href='hybrid:PlayChord?conv={Model.conv}&id={id}';\" />\n"));


#line default
#line hidden

#line 88 "CircleView.cshtml"
                                                                                                                                                                                        
                    

#line default
#line hidden

#line 89 "CircleView.cshtml"
                Write(new HtmlString($"\t\t\t\t<text id=\"text-min-{i}\" text-anchor=\"middle\" stroke=\"none\" fill=\"white\" font-size=\"16\" onclick=\"location.href='hybrid:PlayChord?conv={Model.conv}&id={id}';\">{id}</text>\n"));


#line default
#line hidden

#line 89 "CircleView.cshtml"
                                                                                                                                                                                                                                        
                }


#line default
#line hidden
WriteLiteral("                ");


#line 91 "CircleView.cshtml"
                 for(var i = 1; i <= 12; i++)
                {
                    var idx = (Model.index + i - 1) % 12;
					var id = Model.DiminishedCircle[Model.conv][idx];
                    var color = i == 1 ? "#4d89d5" : "black";
                    
                    

#line default
#line hidden

#line 97 "CircleView.cshtml"
                Write(new HtmlString($"\t\t\t\t<path id=\"arc-dim-{i}\" fill=\"none\" stroke=\"{color}\" onclick=\"location.href='hybrid:PlayChord?conv={Model.conv}&id={id}';\" />\n"));


#line default
#line hidden

#line 97 "CircleView.cshtml"
                                                                                                                                                                                        
                    

#line default
#line hidden

#line 98 "CircleView.cshtml"
                Write(new HtmlString($"\t\t\t\t<text id=\"text-dim-{i}\" text-anchor=\"middle\" stroke=\"none\" fill=\"white\" font-size=\"14\" onclick=\"location.href='hybrid:PlayChord?conv={Model.conv}&id={id}';\">{id}</text>\n"));


#line default
#line hidden

#line 98 "CircleView.cshtml"
                                                                                                                                                                                                                                        
                }


#line default
#line hidden
WriteLiteral("                ");


#line 100 "CircleView.cshtml"
                 for(var i = 1; i <= 12; i++)
                {
                    

#line default
#line hidden

#line 102 "CircleView.cshtml"
                Write(new HtmlString($"\t\t\t\t<line id=\"line-{i}\" />\n"));


#line default
#line hidden

#line 102 "CircleView.cshtml"
                                                                            
                }


#line default
#line hidden
WriteLiteral("\t\t\t\t<text");

WriteLiteral(" id=\"signature\"");

WriteLiteral(" x=\"160\"");

WriteLiteral(" y=\"170\"");

WriteLiteral(" fill=\"#4d89d5\"");

WriteLiteral(" stroke=\"none\"");

WriteLiteral(" font-size=\"32\"");

WriteLiteral(" text-anchor=\"middle\"");

WriteLiteral(">");


#line 104 "CircleView.cshtml"
                                                                                                                  Write(Model.Signature);


#line default
#line hidden
WriteLiteral("</text>\n");


#line 105 "CircleView.cshtml"
				

#line default
#line hidden

#line 105 "CircleView.cshtml"
                 if (Model.index > 1)
				{


#line default
#line hidden
WriteLiteral("\t\t\t\t    <path");

WriteLiteral(" id=\"prev\"");

WriteLiteral(" fill=\"#4d89d5\"");

WriteLiteral(" stroke=\"none\"");

WriteAttribute ("onclick", " onclick=\"", "\""
, Tuple.Create<string,object,bool> ("", "location.href=\'hybrid:Circle?conv=", true)

#line 107 "CircleView.cshtml"
                                                                     , Tuple.Create<string,object,bool> ("", Model.conv

#line default
#line hidden
, false)
, Tuple.Create<string,object,bool> ("", "&index=", true)

#line 107 "CircleView.cshtml"
                                                                                        , Tuple.Create<string,object,bool> ("", Model.index-1

#line default
#line hidden
, false)
, Tuple.Create<string,object,bool> ("", "\';", true)
);
WriteLiteral(" />\n");


#line 108 "CircleView.cshtml"
				}


#line default
#line hidden
WriteLiteral("\t\t\t\t");


#line 109 "CircleView.cshtml"
                 if (Model.index < 15)
				{


#line default
#line hidden
WriteLiteral("\t\t\t\t    <path");

WriteLiteral(" id=\"next\"");

WriteLiteral(" fill=\"#4d89d5\"");

WriteLiteral(" stroke=\"none\"");

WriteAttribute ("onclick", " onclick=\"", "\""
, Tuple.Create<string,object,bool> ("", "location.href=\'hybrid:Circle?conv=", true)

#line 111 "CircleView.cshtml"
                                                                     , Tuple.Create<string,object,bool> ("", Model.conv

#line default
#line hidden
, false)
, Tuple.Create<string,object,bool> ("", "&index=", true)

#line 111 "CircleView.cshtml"
                                                                                        , Tuple.Create<string,object,bool> ("", Model.index+1

#line default
#line hidden
, false)
, Tuple.Create<string,object,bool> ("", "\';", true)
);
WriteLiteral(" />\n");


#line 112 "CircleView.cshtml"
				}


#line default
#line hidden
WriteLiteral("\t\t\t</svg>\n\t\t</div>\n        <script");

WriteLiteral(" type=\"text/javascript\"");

WriteLiteral(">\n\t\t\tfunction polarToCartesian(centerX, centerY, radius, angleInDegrees) {\n\t\t\t  v" +
"ar angleInRadians = (angleInDegrees-90) * Math.PI / 180.0;\n\n\t\t\t  return {\n\t\t\t   " +
" x: centerX + (radius * Math.cos(angleInRadians)),\n\t\t\t    y: centerY + (radius *" +
" Math.sin(angleInRadians))\n\t\t\t  };\n\t\t\t}\n\n\t\t\tfunction describeArc(x, y, radius, s" +
"tartAngle, endAngle){\n\n\t\t\t    var start = polarToCartesian(x, y, radius, endAngl" +
"e);\n\t\t\t    var end = polarToCartesian(x, y, radius, startAngle);\n\n\t\t\t    var lar" +
"geArcFlag = endAngle - startAngle <= 180 ? \"0\" : \"1\";\n\n\t\t\t    var d = [\n\t\t\t     " +
"   \"M\", start.x, start.y, \n\t\t\t        \"A\", radius, radius, 0, largeArcFlag, 0, e" +
"nd.x, end.y\n\t\t\t    ].join(\" \");\n\n\t\t\t    return d;       \n\t\t\t}\n\n\t\t\tfunction playC" +
"hord(value) {\n                $.ajax({\n                    url: \'hybrid:PlayChor" +
"d\',\n                    type: \'get\',\n                    data: {\n               " +
"         id: value\n                    }\n                });\n\t\t\t}\n\n\t\t\t$(document" +
").ready(function() {\n\t\t\t\tvar x = 160;\n\t\t\t\tvar y = 160;\n\n\t\t\t\tfor(var i = 1; i <= " +
"12; i++) {\n\t\t\t\t    var midAngle = (i - 1) * 360 / 12;\n                    var st" +
"artAngle = ((i - 1.5) * 360) / 12;\n\t\t\t\t    var endAngle = ((i - 0.5) * 360) / 12" +
";\n\n\t\t\t\t    $(\'#arc-maj-\' + i).attr(\'d\', describeArc(x, y, 135, startAngle, endAn" +
"gle));\n\t\t\t\t    $(\'#arc-min-\' + i).attr(\'d\', describeArc(x, y, 100, startAngle, e" +
"ndAngle));\n\t\t\t\t    $(\'#arc-dim-\' + i).attr(\'d\', describeArc(x, y, 65, startAngle" +
", endAngle));\n                    \n\t\t\t\t    $(\'#arc-maj-\' + i).attr(\'stroke-width" +
"\', \'30\');\n\t\t\t\t    $(\'#arc-min-\' + i).attr(\'stroke-width\', \'30\');\n\t\t\t\t    $(\'#arc" +
"-dim-\' + i).attr(\'stroke-width\', \'30\');\n\n\t\t\t\t    var end = polarToCartesian(x, y" +
", 160, endAngle);\n                \n                    $(\'#line-\' + i).attr(\'x1\'" +
", x);\n                    $(\'#line-\' + i).attr(\'y1\', y);\n                    $(\'" +
"#line-\' + i).attr(\'x2\', end.x);\n                    $(\'#line-\' + i).attr(\'y2\', e" +
"nd.y);\n\t\t\t\t    $(\'#line-\' + i).attr(\'stroke\', \'white\');\n\t\t\t\t    $(\'#line-\' + i)." +
"attr(\'fill\', \'white\');\n\t\t\t\t    $(\'#line-\' + i).attr(\'stroke-width\', 3);\n\n\t\t\t\t   " +
" var pos = polarToCartesian(x, y, 125, midAngle);\n\t\t\t\t    $(\'#text-maj-\' + i).at" +
"tr(\'transform\', \'translate(\' + pos.x + \', \' + pos.y + \') rotate(\' + midAngle + \'" +
")\');\n\t\t\t\t    pos = polarToCartesian(x, y, 95, midAngle);\n\t\t\t\t    $(\'#text-min-\' " +
"+ i).attr(\'transform\', \'translate(\' + pos.x + \', \' + pos.y + \') rotate(\' + midAn" +
"gle + \')\');\n\t\t\t\t    pos = polarToCartesian(x, y, 60, midAngle);\n\t\t\t\t    $(\'#text" +
"-dim-\' + i).attr(\'transform\', \'translate(\' + pos.x + \', \' + pos.y + \') rotate(\' " +
"+ midAngle + \')\');\n\t\t\t\t}\n\t\t\t\tvar arrowSize = 40;\n\t\t\t\tvar prev = [\n\t\t\t\t    \"M\", 0" +
", 2 * y - arrowSize / 2,\n\t\t\t\t    \"l\", arrowSize, -arrowSize / 2,\n\t\t\t\t    \"v \", a" +
"rrowSize,\n\t\t\t\t    \"Z\"\n\t\t\t\t].join(\" \");\n                var next = [\n            " +
"        \"M\", 2 * x, 2 * y - arrowSize / 2,\n                    \"l\", -arrowSize, " +
"arrowSize / 2,\n                    \"v \", -arrowSize,\n                    \"Z\"\n   " +
"             ].join(\" \");\n\t\t\t\t\n\t\t\t\t$(\'#prev\').attr(\'d\', prev);\n\t\t\t\t$(\'#next\').at" +
"tr(\'d\', next);\n\t\t\t});\n\t\t</script>\n        <!-- Main content end -->\n            " +
"\n    </div>\n    <hr />\n    <footer>\n        <p>&copy; 2017 - Tomáš Němec</p>\n   " +
" </footer>\n</body>\n</html>\n");

}
}
}
#pragma warning restore 1591
