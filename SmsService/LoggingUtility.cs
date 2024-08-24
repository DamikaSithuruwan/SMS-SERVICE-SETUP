using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmsService
{
    public static class LoggingUtility
    {
        private static readonly string journalFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LOGS", "JOURNAL");
        private static readonly string errorFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LOGS", "ERROR");

        static LoggingUtility()
        {
            if(!Directory.Exists(journalFilePath))
            {
                Directory.CreateDirectory(journalFilePath);
            }

            if (!Directory.Exists(errorFilePath))
            {
                Directory.CreateDirectory(errorFilePath);
            }
        }
        public static void LogMessageSent(int id, string phoneNumber, string message)
        {
            string logFileName = $"Journal_{DateTime.Now:yyyy-MM-dd}.txt";
            string logFilePath = Path.Combine(journalFilePath, logFileName);
            string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Message Sent Successfully to {phoneNumber}.Message: {message}";

            File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
        }

        public static void LogError(Exception ex)
        {
            string logFileName = $"Error_{DateTime.Now:yyyy-MM-dd}.txt";
            string logFilePath = Path.Combine(errorFilePath, logFileName);
            string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Error: {ex.Message}";

            File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
        }
    }
}
