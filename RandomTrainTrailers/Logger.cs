using System.Text;

namespace RandomTrainTrailers
{
    internal class DeferredLogger
    {
        private readonly StringBuilder _sb = new StringBuilder();

        public int Length => _sb.Length;

        public void Clear()
        {
            _sb.Length = 0;
        }

        public void Add(string message)
        {
            _sb.AppendLine(message);
        }

        public void Log()
        {
            Util.Log(_sb.ToString());
        }

        public void LogError()
        {
            Util.LogError(_sb.ToString());
        }

        public void LogWarning()
        {
            Util.LogWarning(_sb.ToString());
        }
    }
}
