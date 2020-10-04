using ColorCode.Styling;
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
        public SyntaxHighlightingRichEditBox()
        {
            var formatter = new RichEditBoxFormatter(StyleDictionary.DefaultDark);
            formatter.AttachRichEditBox(this, new BoopPseudoLanguage());

            KeyDown += OnKeyDown;

            DisabledFormattingAccelerators = DisabledFormattingAccelerators.All;
            IsSpellCheckEnabled = false;
        }

        private void OnKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Tab)
            {
                Document.Selection.TypeText("\t");
                e.Handled = true;
            }
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
            Document.Selection.SetText(TextSetOptions.None, text);
        }
    }
}
