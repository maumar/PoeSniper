using System;

namespace PoeSniper
{
    public enum LogLevel
    {
        Information = 1,
        Warning = 2,
        Error = 3,
    };

    public class Logger
    {
        private LogLevel _logLevel = LogLevel.Information;

        public Logger(LogLevel logLevel)
        {
            _logLevel = logLevel;
        }

        public void Information(string message, bool newLine = true)
        {
            if (_logLevel >= LogLevel.Information)
            {
                LogInternal(message, ConsoleColor.DarkGray, "", newLine);
            }
        }

        public void Warning(string message, bool newLine = true)
        {
            if (_logLevel >= LogLevel.Warning)
            {
                LogInternal(message, ConsoleColor.Yellow, "WARNING - ", newLine);
            }
        }

        public void Error(string message, bool newLine = true)
        {
            if (_logLevel >= LogLevel.Warning)
            {
                LogInternal(message, ConsoleColor.Red, "ERROR - ", newLine);
            }
        }

        public void MatchFound(string message, bool newLine = true)
        {
            LogInternal(message, ConsoleColor.Green, "MATCH FOUND - ", newLine);
        }

        private static void LogInternal(string message, ConsoleColor color, string messageTypePrefix, bool newLine)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            if (newLine)
            {
                Console.WriteLine(messageTypePrefix + message);
            }
            else
            {
                Console.Write(messageTypePrefix + message);
            }

            Console.ForegroundColor = oldColor;

        }
    }
}
