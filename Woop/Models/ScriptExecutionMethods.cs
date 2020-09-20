using System;

namespace Woop.Models
{
    public class ScriptExecutionMethods
    {
        private readonly Action<string> _postInfo;
        private readonly Action<string> _postError;
        private readonly Action<string> _insert;

        public ScriptExecutionMethods(Action<string> postInfo, Action<string> postError, Action<string> insert)
        {
            _postInfo = postInfo;
            _postError = postError;
            _insert = insert;
        }

        public void PostError(string error)
        {
            _postError(error);
        }

        public void PostInfo(string info)
        {
            _postInfo(info);
        }

        public void Insert(string newValue)
        {
            _insert(newValue);
        }
    }
}
