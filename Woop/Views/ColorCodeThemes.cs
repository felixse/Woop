using ColorCode.Common;
using ColorCode.Styling;

namespace Woop.Views
{
    public static class ColorCodeThemes
    {
        private const string Redey = "#ffff4c7c";
        private const string Bluish = "#ff3586ff";
        private const string Cyanish = "#ff4cffb2";
        private const string Greenish = "#ff32ff47";
        private const string Orangeish = "#ffffb24c";
        private const string Yellowishy = "#fffcff4c";
        private const string CommentGrey = "#ff9bcce3";
        private const string RedButDarker = "#ffb91843";
        private const string BlueButDarker = "#ff006ab7";
        private const string GreenButDarker = "#ff00b200";
        private const string PurpleButDarker = "#ff760076";
        private const string OrangeABitDarker = "#ffff652d";
        private const string CyanTinyBitDarker = "#ff00b1b7";
        private const string CommentGreyDarkest = "#ff888ea6";
        private const string PurpleButItsLighter = "#fff8a5f8";
        private const string White = "#ffffffff";
        private const string Black = "#ff000000";

        public static StyleDictionary Light = new StyleDictionary
        {
            new Style(ScopeName.Comment)
            {
                Foreground = CommentGreyDarkest
            },
            new Style(ScopeName.String)
            {
                Foreground = RedButDarker
            },
            new Style(ScopeName.Attribute)
            {
                Foreground = CyanTinyBitDarker
            },
            new Style(ScopeName.Number)
            {
                Foreground = OrangeABitDarker
            },
            new Style(ScopeName.PseudoKeyword)
            {
                Foreground = BlueButDarker
            },
            new Style(ScopeName.Keyword)
            {
                Foreground = GreenButDarker
            },
            new Style(ScopeName.PlainText)
            {
                Foreground = Black
            }
        };

        public static StyleDictionary Dark = new StyleDictionary
        {
            new Style(ScopeName.Comment)
            {
                Foreground = CommentGreyDarkest
            },
            new Style(ScopeName.String)
            {
                Foreground = Redey
            },
            new Style(ScopeName.Attribute)
            {
                Foreground = Cyanish
            },
            new Style(ScopeName.Number)
            {
                Foreground = Orangeish
            },
            new Style(ScopeName.PseudoKeyword)
            {
                Foreground = Bluish
            },
            new Style(ScopeName.Keyword)
            {
                Foreground = Greenish
            },
            new Style(ScopeName.PlainText)
            {
                Foreground = White
            }
        };
    }
}
