using System;
using System.IO;
using System.Text;

namespace Fov_Menu
{
    internal static class Logger
    {
        private static readonly object _lock = new();
        private static readonly string _logPath = Path.Combine(AppContext.BaseDirectory, "fov_menu.log");

        public static void LogInfo(string message)
        {
            Write("INFO", message);
        }

        public static void LogError(string message, Exception? ex = null)
        {
            var msg = ex == null ? message : message + "\n" + ex;
            Write("ERROR", msg);
        }

        private static void Write(string level, string message)
        {
            try
            {
                var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {level}: {message}" + Environment.NewLine;
                lock (_lock)
                {
                    File.AppendAllText(_logPath, line, Encoding.UTF8);
                }
            }
            catch
            {
                // Swallow logging errors to avoid affecting app flow
            }
        }
    }
}
