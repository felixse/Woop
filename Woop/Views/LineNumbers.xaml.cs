using Microsoft.Toolkit.Uwp.UI.Animations;
using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

namespace Woop.Views
{
    /// <summary>
    /// All credits go to https://github.com/JasonStein/Notepads/
    /// </summary>
    public sealed partial class LineNumbers : UserControl
    {
        private readonly IList<TextBlock> _renderedLineNumberBlocks = new List<TextBlock>();
        private ScrollBar _contentScrollViewerVerticalScrollBar;

        private readonly Dictionary<string, double> _miniRequisiteIntegerTextRenderingWidthCache = new Dictionary<string, double>();

        private bool _isDocumentLinesCachePendingUpdate = true;
        private string[] _documentLinesCache; // internal copy of the active document text in array format
        private string _document = string.Empty; // internal copy of the active document text

        private const char RichEditBoxDefaultLineEnding = '\r';

        public LineNumbers()
        {
            this.InitializeComponent();

            Container.SizeChanged += OnLineNumberGridSizeChanged;
        }

        private void OnVerticalScrollBarValueChanged(object sender, RangeBaseValueChangedEventArgs _)
        {
            // Make sure line number canvas is in sync with editor's ScrollViewer
            Buffer.ScrollViewer.StartExpressionAnimation(Canvas, sourceAxis: Axis.Y, targetAxis: Axis.Y);
        }

        private void OnContentScrollViewerViewChanged(object sender, ScrollViewerViewChangedEventArgs _)
        {
            UpdateLineNumbersRendering();
        }

        private void OnContentScrollViewerSizeChanged(object sender, SizeChangedEventArgs _)
        {
            UpdateLineNumbersRendering();
        }

        private void ResetLineNumberCanvasClipping()
        {
            Container.Margin = new Thickness(0, 0, (-1 * Padding.Left) + 1, 0);
            Container.Clip = new RectangleGeometry
            {
                Rect = new Rect(
                    0,
                    Padding.Top,
                    Container.ActualWidth,
                    Math.Clamp(Container.ActualHeight - (Padding.Top + Padding.Bottom), .0f, Double.PositiveInfinity))
            };
        }

        private void OnLineNumberGridSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ResetLineNumberCanvasClipping();
        }

        public void Initialize(SyntaxHighlightingRichEditBox buffer)
        {
            Buffer = buffer;

            Buffer.TextChanged += OnBufferTextChanged;
            Buffer.TextChanging += OnBufferTextChanging;

            Buffer.ScrollViewer.ViewChanged += OnContentScrollViewerViewChanged;
            Buffer.ScrollViewer.SizeChanged += OnContentScrollViewerSizeChanged;

            var scrollViewerRoot = (FrameworkElement)VisualTreeHelper.GetChild(Buffer.ScrollViewer, 0);
            _contentScrollViewerVerticalScrollBar = (ScrollBar)scrollViewerRoot.FindName("VerticalScrollBar");


            if (_contentScrollViewerVerticalScrollBar != null)
            {
                _contentScrollViewerVerticalScrollBar.ValueChanged += OnVerticalScrollBarValueChanged;
            }

            UpdateLineNumbersRendering();
        }

        private void OnBufferTextChanging(RichEditBox sender, RichEditBoxTextChangingEventArgs args)
        {
            if (args.IsContentChanging)
            {
                Buffer.Document.GetText(TextGetOptions.None, out var document);
                _document = TrimRichEditBoxText(document);
                _isDocumentLinesCachePendingUpdate = true;
            }
        }

        private static string TrimRichEditBoxText(string text)
        {
            // Trim end \r
            if (!string.IsNullOrEmpty(text) && text[text.Length - 1] == RichEditBoxDefaultLineEnding)
            {
                text = text.Substring(0, text.Length - 1);
            }

            return text;
        }

        private void OnBufferTextChanged(object sender, RoutedEventArgs e)
        {
            UpdateLineNumbersRendering();
        }

        public SyntaxHighlightingRichEditBox Buffer { get; private set; }

        public string GetText()
        {
            return _document;
        }

        private string[] GetDocumentLinesCache()
        {
            if (_isDocumentLinesCachePendingUpdate)
            {
                _documentLinesCache = (GetText() + RichEditBoxDefaultLineEnding).Split(RichEditBoxDefaultLineEnding);
                _isDocumentLinesCachePendingUpdate = false;
            }

            return _documentLinesCache;
        }

        private Dictionary<int, Rect> CalculateLineNumberTextRenderingPositions(string[] lines, ITextRange startRange, ITextRange endRange)
        {
            var offset = 0;
            var lineRects = new Dictionary<int, Rect>(); // 1 - based

            for (int i = 0; i < lines.Length - 1; i++)
            {
                var line = lines[i];

                // Use "offset + line.Length + 1" instead of just "offset" here is to capture the line right above the viewport
                if (offset + line.Length + 1 >= startRange.StartPosition && offset <= endRange.EndPosition)
                {
                    Buffer.Document.GetRange(offset, offset + line.Length)
                        .GetRect(PointOptions.ClientCoordinates, out var rect, out _);

                    lineRects[i + 1] = rect;
                }
                else if (offset > endRange.EndPosition)
                {
                    break;
                }

                offset += line.Length + 1; // 1 for line ending: '\r'
            }

            return lineRects;
        }

        /// <summary>
        /// Get minimum rendering width needed for displaying number text with certain length.
        /// Take length of 3 as example, it is going to iterate thru all possible combinations like:
        /// 111, 222, 333, 444 ... 999 to get minimum rendering length needed to display all of them (the largest width is the min here).
        /// For mono font text, the width is always the same for same length but for non-mono font text, it depends.
        /// Thus we need to calculate here to determine width needed for rendering integer number only text.
        /// </summary>
        /// <param name="fontFamily"></param>
        /// <param name="fontSize"></param>
        /// <param name="numberTextLength"></param>
        /// <returns></returns>
        private double CalculateMinimumRequisiteIntegerTextRenderingWidth(FontFamily fontFamily, double fontSize, int numberTextLength)
        {
            var cacheKey = $"{fontFamily.Source}-{(int)fontSize}-{numberTextLength}";

            if (_miniRequisiteIntegerTextRenderingWidthCache.ContainsKey(cacheKey))
            {
                return _miniRequisiteIntegerTextRenderingWidthCache[cacheKey];
            }

            double minRequisiteWidth = 0;

            for (int i = 0; i < 10; i++)
            {
                var str = new string((char)('0' + i), numberTextLength);
                var width = GetTextSize(fontFamily, fontSize, str).Width;
                if (width > minRequisiteWidth)
                {
                    minRequisiteWidth = width;
                }
            }

            _miniRequisiteIntegerTextRenderingWidthCache[cacheKey] = minRequisiteWidth;
            return minRequisiteWidth;
        }

        public static Size GetTextSize(FontFamily font, double fontSize, string text)
        {
            var tb = new TextBlock { Text = text, FontFamily = font, FontSize = fontSize };
            tb.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            return tb.DesiredSize;
        }

        private void UpdateLineNumbersRendering()
        {
            var startRange = Buffer.Document.GetRangeFromPoint(
                new Point(Buffer.ScrollViewer.HorizontalOffset, Buffer.ScrollViewer.VerticalOffset),
                PointOptions.ClientCoordinates);

            var endRange = Buffer.Document.GetRangeFromPoint(
                new Point(Buffer.ScrollViewer.HorizontalOffset + Buffer.ScrollViewer.ViewportWidth,
                    Buffer.ScrollViewer.VerticalOffset + Buffer.ScrollViewer.ViewportHeight),
                PointOptions.ClientCoordinates);

            var document = GetDocumentLinesCache();

            Dictionary<int, Rect> lineNumberTextRenderingPositions = CalculateLineNumberTextRenderingPositions(document, startRange, endRange);

            var minLineNumberTextRenderingWidth = CalculateMinimumRequisiteIntegerTextRenderingWidth(new FontFamily("Consolas"),
                FontSize, (document.Length - 1).ToString().Length);

            RenderLineNumbersInternal(lineNumberTextRenderingPositions, minLineNumberTextRenderingWidth);
        }

        public double GetSingleLineHeight()
        {
            Buffer.Document.GetRange(0, 0).GetRect(PointOptions.ClientCoordinates, out var rect, out _);
            return rect.Height <= 0 ? 1.35 * FontSize : rect.Height;
        }

        private void RenderLineNumbersInternal(Dictionary<int, Rect> lineNumberTextRenderingPositions, double minLineNumberTextRenderingWidth)
        {
            var padding = FontSize / 2;
            var lineNumberPadding = new Thickness(padding, 5, padding + 2, 5);
            var lineHeight = GetSingleLineHeight();

            var numOfReusableLineNumberBlocks = _renderedLineNumberBlocks.Count;

            foreach (var (lineNumber, rect) in lineNumberTextRenderingPositions)
            {
                var margin = new Thickness(lineNumberPadding.Left,
                    rect.Top + lineNumberPadding.Top + Padding.Top,
                    lineNumberPadding.Right,
                    lineNumberPadding.Bottom);

                // Re-use already rendered line number blocks
                if (numOfReusableLineNumberBlocks > 0)
                {
                    var index = numOfReusableLineNumberBlocks - 1;
                    _renderedLineNumberBlocks[index].Text = lineNumber.ToString();
                    _renderedLineNumberBlocks[index].Margin = margin;
                    _renderedLineNumberBlocks[index].Height = lineHeight;
                    _renderedLineNumberBlocks[index].Width = minLineNumberTextRenderingWidth;
                    _renderedLineNumberBlocks[index].Visibility = Visibility.Visible;
                    _renderedLineNumberBlocks[index].Opacity = 0.5;
                    _renderedLineNumberBlocks[index].FontFamily = new FontFamily("Consolas");

                    numOfReusableLineNumberBlocks--;
                }
                else // Render new line number block when there is nothing to re-use
                {
                    var lineNumberBlock = new TextBlock()
                    {
                        Text = lineNumber.ToString(),
                        Height = lineHeight,
                        Width = minLineNumberTextRenderingWidth,
                        Margin = margin,
                        TextAlignment = TextAlignment.Right,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Bottom,
                        HorizontalTextAlignment = TextAlignment.Right,
                        Opacity = 0.5,
                        FontFamily = new FontFamily("Consolas")
                    };

                    Canvas.Children.Add(lineNumberBlock);
                    _renderedLineNumberBlocks.Add(lineNumberBlock);
                }
            }

            // Hide all un-used rendered line number block to avoid rendering collision from happening
            for (int i = 0; i < numOfReusableLineNumberBlocks; i++)
            {
                _renderedLineNumberBlocks[i].Visibility = Visibility.Collapsed;
            }

            Container.BorderThickness = new Thickness(0, 0, 0.08 * lineHeight, 0);
            Container.Width = lineNumberPadding.Left + minLineNumberTextRenderingWidth + lineNumberPadding.Right;
        }
    }
}
