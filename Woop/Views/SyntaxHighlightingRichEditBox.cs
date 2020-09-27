using ColorCode.Styling;
using Windows.System;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Woop.Models;
using Woop.Services;

namespace Woop.Views
{
    public class SyntaxHighlightingRichEditBox : RichEditBox
    {
        public SyntaxHighlightingRichEditBox()
        {
            var formatter = new RichEditBoxFormatter(StyleDictionary.DefaultDark);
            formatter.AttachRichEditBox(this, new BoopPseudoLanguage());

            TextChanged += OnTextChanged;
            SelectionChanged += OnSelectionChanged;
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

        private void OnSelectionChanged(object sender, RoutedEventArgs e)
        {
            Document.Selection.GetText(TextGetOptions.UseCrlf, out var selectedText);
            SelectedText = new Selection
            {
                Content = selectedText,
                Start = Document.Selection.StartPosition,
                Length = Document.Selection.Length
            };
        }

        private void OnTextChanged(object sender, RoutedEventArgs e)
        {
            Document.GetText(TextGetOptions.UseCrlf, out var value);
            if (string.IsNullOrEmpty(value))
            {
                return;
            }
            Content = value;
        }

        public string Content
        {
            get { return (string)GetValue(ContentProperty); }
            set
            {
                SetValue(ContentProperty, value);
            }
        }

        public Selection SelectedText
        {
            get { return (Selection)GetValue(SelectedTextProperty); }
            set
            {
                SetValue(SelectedTextProperty, value);
            }
        }

        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register(nameof(Content), typeof(string), typeof(SyntaxHighlightingRichEditBox), new PropertyMetadata(null, OnContentChanged));

        public static readonly DependencyProperty SelectedTextProperty =
            DependencyProperty.Register(nameof(SelectedText), typeof(Selection), typeof(SyntaxHighlightingRichEditBox), new PropertyMetadata(null));

        private static void OnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as SyntaxHighlightingRichEditBox;
            if (e.NewValue != e.OldValue)
            {
                var selection = control.Document.Selection;
                var selectionStart = selection.StartPosition;
                var selectionEnd = selection.EndPosition;

                control.Document.SetText(TextSetOptions.None, e.NewValue.ToString());

                var newSelection = control.Document.Selection;
                newSelection.StartPosition = selectionStart;
                newSelection.EndPosition = selectionEnd;
            }
        }
    }
}
