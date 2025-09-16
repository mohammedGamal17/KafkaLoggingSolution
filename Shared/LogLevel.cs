namespace Shared
{
    public enum LoggingLevel
    {
        Info = 1,
        Warning,
        Error,
        Critical,
        Debug,
        Success
    }
    public static class LogLevelExtensions
    {
        public static string ToSt(this LoggingLevel logLevel)
        {
            return logLevel.ToString();
        }
    }
}
