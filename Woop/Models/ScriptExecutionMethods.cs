using System;

namespace Woop.Models
{
    public class ScriptExecutionMethods
    {
        private readonly Action<string> _postInfo;
        private readonly Action<string> _postError;

        public ScriptExecutionMethods(Action<string> postInfo, Action<string> postError)
        {
            _postInfo = postInfo;
            _postError = postError;
        }

        public void PostError(string error)
        {
            _postError(error);
        }

        public void PostInfo(string info)
        {
            _postInfo(info);
        }
    }
}
