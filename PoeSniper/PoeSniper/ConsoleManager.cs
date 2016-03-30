using System;

namespace PoeSniper
{
    public static class Logger
    {
        public static void Information(string message)
        {
            LogInternal(message, ConsoleColor.DarkGray, "");
        }

        public static void Warning(string message)
        {
            LogInternal(message, ConsoleColor.Yellow, "WARNING - ");
        }

        public static void Error(string message)
        {
            LogInternal(message, ConsoleColor.Red, "ERROR - ");
        }

        private static void LogInternal(string message, ConsoleColor color, string messageTypePrefix)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(messageTypePrefix + message);
            Console.ForegroundColor = oldColor;

        }
    }
}
