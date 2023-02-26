using ColorCode;
using ColorCode.Common;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Woop.Services
{
    public class BoopPseudoLanguage : ILanguage
    {
        private readonly IEnumerable<string> _commonAttributes = new string[] { "var", "val", "let", "if", "else", "export", "import", "return", "static", "fun", "function", "func", "class", "open", "new", "as", "where", "select", "delete", "add", "limit", "update", "insert" };

        private readonly IEnumerable<string> _moreAttributes = new string[] { "true", "false", "to", "string", "int", "float", "double", "bool", "boolean", "from" };

        private readonly LanguageRule _singleLineComment;
        private readonly LanguageRule _multiLineComment;
        private readonly LanguageRule _htmlComment;
        private readonly LanguageRule _number;
        private readonly LanguageRule _keywords;
        private readonly LanguageRule _attributes;
        private readonly LanguageRule _utcDates;
        private readonly LanguageRule _md5Hashes;
        private readonly LanguageRule _xmlTags;
        private readonly LanguageRule _jsonLabels;
        private readonly LanguageRule _singleQuotes;
        private readonly LanguageRule _doubleQuotes;
        private readonly LanguageRule _backTicks;
        private readonly LanguageRule _multiLineString;

        public BoopPseudoLanguage()
        {
            _singleLineComment = new LanguageRule(
                @"(//.*?)\r?$",
                new Dictionary<int, string>
                {
                    [1] = ScopeName.Comment
                }
            );

            _multiLineComment = new LanguageRule(
                @"/\*([^*]|[\r\n]|(\*+([^*/]|[\r\n])))*\*+/",
                new Dictionary<int, string>
                {
                    [0] = ScopeName.Comment
                }
            );

            _htmlComment = new LanguageRule(
                @"<!--[\s\S]*?-->",
                new Dictionary<int, string>
                {
                    [0] = ScopeName.Comment
                }
            );

            _number = new LanguageRule(
                @"\b(?:0x[a-f0-9]+|(?:\d(?:_\d+)*\d*(?:\.\d*)?|\.\d\+)(?:e[+\-]?\d+)?)\b",
                new Dictionary<int, string>
                {
                    [0] = ScopeName.Number
                }
            );

            _keywords = new LanguageRule(
                $"(?i)\\b({string.Join('|', _commonAttributes)})\\b",
                new Dictionary<int, string>
                {
                    [0] = ScopeName.Attribute
                }
            );

            _attributes = new LanguageRule(
                $"(?i)\\b({string.Join('|', _moreAttributes)})\\b",
                new Dictionary<int, string>
                {
                    [0] = ScopeName.Keyword
                }
            );

            // todo broken
            _utcDates = new LanguageRule(
                @"(?:(Sun|Mon|Tue|Wed|Thu|Fri|Sat),\\s+)?(0[1-9]|[1-2]?[0-9]|3[01])\\s+(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)\\s+(19[0-9]{2}|[2-9][0-9]{3})\\s+(2[0-3]|[0-1][0-9]):([0-5][0-9])(?::(60|[0-5][0-9]))?\\s+([-\\+][0-9]{2}[0-5][0-9]|(?:UT|GMT|(?:E|C|M|P)(?:ST|DT)|[A-IK-Z]))",
                new Dictionary<int, string>
                {
                    [0] = ScopeName.Number
                }
            );

            _md5Hashes = new LanguageRule(
                "[a-f0-9]{32}",
                new Dictionary<int, string>
                {
                    [0] = ScopeName.Keyword
                }
            );

            // todo changed implementation, sync to Boop?
            _xmlTags = new LanguageRule(
                "(<.[^(><.)]+>)",
                new Dictionary<int, string>
                {
                    [0] = ScopeName.Attribute
                }
            );

            _jsonLabels = new LanguageRule(
                @"""([^""]+?)""\s*(?=:)",
                new Dictionary<int, string>
                {
                    [0] = ScopeName.PseudoKeyword
                }
            );

            _singleQuotes = new LanguageRule(
                @"'[^\n]*?'",
                new Dictionary<int, string>
                {
                    [0] = ScopeName.String
                }
            );

            _doubleQuotes = new LanguageRule(
                @"\""(?:[^\""\\]|\\.)*\""",
                new Dictionary<int, string>
                {
                    [0] = ScopeName.String
                }
            );

            _backTicks = new LanguageRule(
                @"`[^\n]*?`",
                new Dictionary<int, string>
                {
                    [0] = ScopeName.String
                }
            );

            _multiLineString = new LanguageRule(
                @"(?s)(\""\""\"")(.*?)(\""\""\"")",
                new Dictionary<int, string>
                {
                    [0] = ScopeName.String
                }
            );
        }

        public string Id => "Boop";

        public string Name => Id;

        public string CssClassName => Id;

        public string FirstLinePattern => null;

        public IList<LanguageRule> Rules => new List<LanguageRule>
        {
            _multiLineComment,
            _singleLineComment,
            _htmlComment,
            _number,
            _keywords,
            _attributes,
            _utcDates,
            _md5Hashes,
            _xmlTags,
            _jsonLabels,
            _multiLineString,
            _singleQuotes,
            _doubleQuotes,
            _backTicks,
        };

        public bool HasAlias(string lang)
        {
            return false;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
