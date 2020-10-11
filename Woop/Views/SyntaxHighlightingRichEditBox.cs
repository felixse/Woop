using ColorCode;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using Windows.System;
using Windows.UI.Text;
using Windows.UI.Xaml.Controls;
using Woop.Models;
using Woop.Services;
using Woop.ViewModels;

namespace Woop.Views
{
    public class SyntaxHighlightingRichEditBox : RichEditBox, IBuffer
    {
        private readonly RtfFormatter _rtfFormatter;
        private readonly ILanguage _language;

        public SyntaxHighlightingRichEditBox()
        {
            _rtfFormatter = new RtfFormatter(ColorCodeThemes.Light); // todo dynamic
            _language = new BoopPseudoLanguage();

            KeyDown += OnKeyDown;
            TextChanging += OnTextChanging;

            DisabledFormattingAccelerators = DisabledFormattingAccelerators.All;
            IsSpellCheckEnabled = false;
        }

        private void OnTextChanging(RichEditBox sender, RichEditBoxTextChangingEventArgs args)
        {
            if (args.IsContentChanging)
            {
                UpdateText();
            }
        }

        private void OnKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Tab)
            {
                Document.Selection.TypeText("\t");
                e.Handled = true;
            }
        }

        public void UpdateText()
        {
            // Attempt to get Scrollviewer offsets, to preserve location.
            var scrollViewer = this.FindDescendant<ScrollViewer>();
            var vertOffset = scrollViewer?.VerticalOffset;
            var horOffset = scrollViewer?.HorizontalOffset;

            var selection = Document.Selection;
            var selectionStart = selection.StartPosition;
            var selectionEnd = selection.EndPosition;

            Document.GetText(TextGetOptions.UseCrlf, out string raw);
            Document.Undo();
            Document.BeginUndoGroup();
            Document.SetText(TextSetOptions.None, raw);

            var rtf = _rtfFormatter.GetRtfString(raw, _language);

            Document.SetText(TextSetOptions.FormatRtf, rtf);

            var newSelection = Document.Selection;
            newSelection.StartPosition = selectionStart;
            newSelection.EndPosition = selectionEnd;

            Document.ApplyDisplayUpdates();
            Document.EndUndoGroup();

            scrollViewer?.ChangeView(horOffset, vertOffset, null, true);
        }

        string IBuffer.GetText()
        {
            Document.GetText(TextGetOptions.UseCrlf, out var text);
            return text;
        }

        void IBuffer.SetText(string text)
        {
            Document.SetText(TextSetOptions.None, text);
        }

        Selection IBuffer.GetSelection()
        {
            Document.Selection.GetText(TextGetOptions.UseCrlf, out var selectedText);
            return new Selection
            {
                Content = selectedText,
                Start = Document.Selection.StartPosition,
                Length = Document.Selection.Length
            };
        }

        void IBuffer.SetSelection(string text)
        {
            Document.Selection.SetText(TextSetOptions.None, text ?? string.Empty);
        }
    }
}
