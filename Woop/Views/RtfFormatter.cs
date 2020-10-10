using ColorCode;
using ColorCode.Common;
using ColorCode.Parsing;
using ColorCode.Styling;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Windows.UI;

namespace Woop.Views
{
    public class RtfFormatter : CodeColorizerBase
    {
        private readonly Dictionary<string, int> _colorTableIndexes = new Dictionary<string, int>();
        private readonly string _colorTable;

        public RtfFormatter(StyleDictionary style = null, ILanguageParser languageParser = null) : base(style, languageParser)
        {
            _colorTable = PrepareColorTable();
        }

        private TextWriter Writer { get; set; }

        /// <summary>
        /// Creates the RTF Markup, which can be saved to a .rtf file.
        /// </summary>
        /// <param name="sourceCode">The source code to colorize.</param>
        /// <param name="language">The language to use to colorize the source code.</param>
        /// <returns>Colorised RTF Markup.</returns>
        public string GetRtfString(string sourceCode, ILanguage language)
        {
            var buffer = new StringBuilder(sourceCode.Length * 2);

            using (var writer = new StringWriter(buffer))
            {
                Writer = writer;
                WriteHeader();

                languageParser.Parse(sourceCode, language, Write);

                writer.Flush();
            }

            return buffer.ToString();
        }

        protected override void Write(string parsedSourceCode, IList<Scope> scopes)
        {
            var styleInsertions = new List<TextInsertion>();

            if (scopes.Count == 0)
            {
                Writer.Write("\\cf0 ");
                Writer.Write(RtfEncodeText(parsedSourceCode));
                return;
            }

            foreach (Scope scope in scopes)
            {
                GetStyleInsertionsForCapturedStyle(scope, styleInsertions);
            }

            styleInsertions.SortStable((x, y) => x.Index.CompareTo(y.Index));

            int offset = 0;

            foreach (TextInsertion styleInsertion in styleInsertions)
            {
                var text = parsedSourceCode.Substring(offset, styleInsertion.Index - offset);
                Writer.Write(RtfEncodeText(text));

                BuildControlWordsForCapturedStyle(styleInsertion.Scope);

                if (string.IsNullOrEmpty(styleInsertion.Text))
                {
                    BuildControlWordsForCapturedStyle(styleInsertion.Scope);
                }
                else
                {
                    Writer.Write(styleInsertion.Text);
                }

                offset = styleInsertion.Index;
            }

            Writer.Write(RtfEncodeText(parsedSourceCode.Substring(offset)));
        }

        private string RtfEncodeText(string text)
        {
            text = text.Replace("\\", "\\\\");
            text = text.Replace("{", @"\{");
            text = text.Replace("}", @"\}");
            text = text.Replace("\r\n", "\\par\r\n");
            return text;
        }

        private void WriteHeader()
        {
            Writer.WriteLine(@"{\rtf1\fbidis\ansi\ansicpg1252\deff0\nouicompat\deflang1031{\fonttbl{\f0\fnil Consolas;}}");
            Writer.Write(_colorTable);
            Writer.Write(@"{\*\generator Riched20 10.0.19041}\viewkind4\uc1\pard\tx720\cf1\f0\fs21\lang1033");
        }

        private string PrepareColorTable()
        {
            var colorTable = new StringBuilder();
            var index = 1;

            colorTable.Append(@"{\colortbl ;");

            foreach (var style in Styles)
            {
                if (style.Foreground == null)
                {
                    continue;
                }

                var color = HexToColor(style.Foreground);
                colorTable.Append($@"\red{color.R}\green{color.G}\blue{color.B};");
                _colorTableIndexes[style.ScopeName] = index++;
            }
            colorTable.AppendLine("}");

            return colorTable.ToString();
        }

        public static Color HexToColor(string hexString)
        {
            if (hexString.IndexOf('#') != -1)
            {
                hexString = hexString.Replace("#", string.Empty);
            }

            byte a, r, g, b = 0;

            a = (byte)int.Parse(hexString.Substring(0, 2), NumberStyles.AllowHexSpecifier);
            r = (byte)int.Parse(hexString.Substring(2, 2), NumberStyles.AllowHexSpecifier);
            g = (byte)int.Parse(hexString.Substring(4, 2), NumberStyles.AllowHexSpecifier);
            b = (byte)int.Parse(hexString.Substring(6, 2), NumberStyles.AllowHexSpecifier);

            return Color.FromArgb(a, r, g, b);
        }

        private void GetStyleInsertionsForCapturedStyle(Scope scope, ICollection<TextInsertion> styleInsertions)
        {
            styleInsertions.Add(new TextInsertion
            {
                Index = scope.Index,
                Scope = scope
            });

            foreach (Scope childScope in scope.Children)
            {
                GetStyleInsertionsForCapturedStyle(childScope, styleInsertions);
            }

            styleInsertions.Add(new TextInsertion
            {
                Index = scope.Index + scope.Length,
                Text = GetEndTag(scope)
            });
        }

        private string GetEndTag(Scope scope)
        {
            var end = string.Empty;
            var style = Styles[scope.Name];

            if (style.Bold)
            {
                end += "\\b0";
            }

            if (style.Italic)
            {
                end += "\\i0";
            }

            return end;
        }

        private void BuildControlWordsForCapturedStyle(Scope scope)
        {
            if (scope == null)
            {
                return;
            }

            var style = Styles[scope.Name];

            if (style.Foreground != null)
            {
                var index = _colorTableIndexes[style.ScopeName];
                Writer.Write($@"\cf{index} ");
            }
            else
            {
                Writer.Write("\\cf0 ");
            }

            if (style.Bold)
            {
                Writer.Write("\\b ");
            }

            if (style.Italic)
            {
                Writer.Write("\\i ");
            }
        }
    }
}
