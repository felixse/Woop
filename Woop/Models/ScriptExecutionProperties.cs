namespace Woop.Models
{
    public struct ScriptExecutionProperties
    {
        public ScriptExecutionProperties(string selection, string fullText, int insertPosition)
        {
            IsSelection = !string.IsNullOrEmpty(selection);
            Selection = selection;
            FullText = fullText;
            InsertPosition = insertPosition;
        }

        public string Selection;
        public string FullText;
        public bool IsSelection;
        public int InsertPosition;
    }
}
