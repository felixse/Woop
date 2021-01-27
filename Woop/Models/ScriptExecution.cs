using System;
using System.Diagnostics.CodeAnalysis;

namespace Woop.Models
{
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Casing defined by Boop API")]
    public class ScriptExecution
    {
        private readonly Action<string> _postInfo;
        private readonly Action<string> _postError;

        public ScriptExecution(string selection, string fullText, int insertIndex, Action<string> postInfo, Action<string> postError)
        {
            isSelection = !string.IsNullOrEmpty(selection);
            this.selection = selection;
            this.fullText = fullText;
            this.insertIndex = insertIndex;

            _postInfo = postInfo;
            _postError = postError;
        }

        public bool isSelection { get; }

        public int insertIndex { get; private set; }

        public string selection { get; set; }

        public string fullText { get; set; }

        public string text
        {
            get => isSelection ? selection : fullText;
            set
            {
                if (isSelection)
                {
                    selection = value;
                }
                else
                {
                    fullText = value;
                }
            }
        }

        public void insert(string newValue)
        {
            if (isSelection)
            {
                selection = newValue;
                return;
            }

            fullText.Insert(insertIndex, newValue);

            insertIndex += newValue.Length;
        }


        public void postError(string error)
        {
            _postError(error);
        }

        public void postInfo(string info)
        {
            _postInfo(info);
        }
    }
}
