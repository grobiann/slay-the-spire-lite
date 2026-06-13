namespace STSLite.Core.Logging
{
    public class UnityLogPrinter : ILogPrinter
    {
        public void Print(LogLevel logLevel, string text, int skipFrames)
        {
            string message = $"[{logLevel.ToString().ToUpperInvariant()}] {text}";
            switch (logLevel)
            {
                case LogLevel.Warn:
                    Debug.LogWarning(message);
                    break;
                case LogLevel.Error:
                    Debug.LogError(message);
                    break;
                default:
                    Debug.Log(message);
                    break;
            }
        }
    }
}