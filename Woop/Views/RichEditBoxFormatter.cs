using System.Collections.Generic;
using ColorCode.Parsing;
using Windows.UI.Xaml.Controls;
using ColorCode.Styling;
using Windows.UI.Text;
using ColorCode;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Toolkit.Uwp.UI.Extensions;

namespace Woop.Views
{
    public class RichEditBoxFormatter : CodeColorizerBase
    {
        private RichEditBox _richEditBox;
        private ILanguage _language;

        private static int Index = 0;
        private static string source = string.Empty;

        public RichEditBoxFormatter(StyleDictionary Style = null, ILanguageParser languageParser = null)
            : base(Style, languageParser)
        {
        }

        public void AttachRichEditBox(RichEditBox RichEdit, ILanguage Language)
        {
            _richEditBox = RichEdit;
            RichEdit.TextChanging += RichEdit_TextChanging;
            _language = Language;
        }

        private void RichEdit_TextChanging(RichEditBox sender, RichEditBoxTextChangingEventArgs args)
        {
            if (args.IsContentChanging)
            {
                UpdateText();
            }
        }

        public void UpdateText()
        {
            if (_richEditBox != null)
            {
                // Attempt to get Scrollviewer offsets, to preserve location.
                var scrollViewer = _richEditBox.FindDescendant<ScrollViewer>();
                var vertOffset = scrollViewer?.VerticalOffset;
                var horOffset = scrollViewer?.HorizontalOffset;

                var selection = _richEditBox.Document.Selection;
                var selectionStart = selection.StartPosition;
                var selectionEnd = selection.EndPosition;

                _richEditBox.Document.GetText(TextGetOptions.UseCrlf, out string raw);
                _richEditBox.Document.Undo();
                _richEditBox.Document.BeginUndoGroup();
                _richEditBox.Document.SetText(TextSetOptions.None, raw);

                var newSelection = _richEditBox.Document.Selection;
                newSelection.StartPosition = selectionStart;
                newSelection.EndPosition = selectionEnd;

                Reset(_richEditBox.TextDocument.GetRange(0, raw.Length));

                Index = 0;
                source = raw;

                languageParser.Parse(raw, _language, StyleRange);
                _richEditBox.Document.ApplyDisplayUpdates();
                _richEditBox.Document.EndUndoGroup();

                scrollViewer?.ChangeView(horOffset, vertOffset, null, true);
            }
        }

        protected void StyleRange(string range, IList<Scope> scopes)
        {
            //Debug.WriteLine($"[{range}]: {scopes.FirstOrDefault()?.Name ?? "None"}");
            var scopeRange = source.Substring(Index);

            var start = Index;

            try
            {
                // todo still somehow odd after multiline ranges
                var previous = source.Remove(Index);
                var crlfCorrection = CountOccurences(previous, "\r\n");
                start -= crlfCorrection;
            }
            catch { }

            var subIndex = scopeRange.IndexOf(range);
            if (subIndex != -1)
            {
                start += subIndex;
            }

            foreach (var scope in scopes)
            {
                StyleFromScope(start, scope);
            }

            Index += range.Length;
        }

        private int CountOccurences(string str, string match)
        {
            return Regex.Matches(str, match).Count;
        }

        private void StyleFromScope(int start, Scope scope)
        {
            start += scope.Index;
            var Range = _richEditBox.Document.GetRange(start, start + scope.Length);

            string foreground = null;
            string background = null;
            bool italic = false;
            bool bold = false;

            if (Styles.Contains(scope.Name))
            {
                var style = Styles[scope.Name];

                foreground = style.Foreground;
                background = style.Background;
                italic = style.Italic;
                bold = style.Bold;
            }

            if (!string.IsNullOrWhiteSpace(foreground))
            {
                Range.CharacterFormat.ForegroundColor = Microsoft.Toolkit.Uwp.Helpers.ColorHelper.ToColor(foreground);
            }

            if (!string.IsNullOrWhiteSpace(background))
            {
                Range.CharacterFormat.BackgroundColor = Microsoft.Toolkit.Uwp.Helpers.ColorHelper.ToColor(background);
            }

            if (italic)
                Range.CharacterFormat.Italic = FormatEffect.On;

            if (bold)
                Range.CharacterFormat.Bold = FormatEffect.On;

            foreach (var subScope in scope.Children)
            {
                StyleFromScope(start, subScope);
            }
        }

        private void Reset(ITextRange Range)
        {
            var defaults = _richEditBox.Document.GetDefaultCharacterFormat();
            Range.CharacterFormat.Italic = FormatEffect.Off;
            Range.CharacterFormat.Bold = FormatEffect.Off;
            Range.CharacterFormat.BackgroundColor = defaults.BackgroundColor;
            Range.CharacterFormat.ForegroundColor = defaults.ForegroundColor;
        }

        protected override void Write(string parsedSourceCode, IList<Scope> scopes)
        {
            throw new System.NotImplementedException();
        }
    }
}

