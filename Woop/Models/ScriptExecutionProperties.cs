namespace Woop.Models
{
    public struct ScriptExecutionProperties
    {
        public ScriptExecutionProperties(string selection, string fullText)
        {
            IsSelection = !string.IsNullOrEmpty(selection);
            Selection = selection;
            FullText = fullText;
        }

        public string Selection;
        public string FullText;
        public bool IsSelection;
    }
}
