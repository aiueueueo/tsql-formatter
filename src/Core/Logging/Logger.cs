using System;
using System.IO;

namespace TSqlFormatter.Core.Logging
{
    /// <summary>
    /// Log levels for the logger.
    /// </summary>
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }

    /// <summary>
    /// Simple file-based logger for the T-SQL Formatter.
    /// </summary>
    public class Logger
    {
        private static readonly Lazy<Logger> _instance = new Lazy<Logger>(() => new Logger());
        private readonly string _logFilePath;
        private readonly object _lock = new object();
        private LogLevel _minLevel = LogLevel.Info;

        /// <summary>
        /// Gets the singleton instance of the logger.
        /// </summary>
        public static Logger Instance => _instance.Value;

        private Logger()
        {
            var logFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "T-SQL Formatter",
                "Logs");

            if (!Directory.Exists(logFolder))
            {
                Directory.CreateDirectory(logFolder);
            }

            _logFilePath = Path.Combine(logFolder, $"tsqlformatter_{DateTime.Now:yyyyMMdd}.log");
        }

        /// <summary>
        /// Gets or sets the minimum log level.
        /// </summary>
        public LogLevel MinLevel
        {
            get => _minLevel;
            set => _minLevel = value;
        }

        /// <summary>
        /// Logs a debug message.
        /// </summary>
        public void Debug(string message) => Log(LogLevel.Debug, message);

        /// <summary>
        /// Logs an info message.
        /// </summary>
        public void Info(string message) => Log(LogLevel.Info, message);

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        public void Warning(string message) => Log(LogLevel.Warning, message);

        /// <summary>
        /// Logs an error message.
        /// </summary>
        public void Error(string message) => Log(LogLevel.Error, message);

        /// <summary>
        /// Logs an exception.
        /// </summary>
        public void Error(string message, Exception ex)
        {
            Log(LogLevel.Error, $"{message}: {ex.GetType().Name} - {ex.Message}");
            Log(LogLevel.Debug, ex.StackTrace ?? "No stack trace available");
        }

        private void Log(LogLevel level, string message)
        {
            if (level < _minLevel) return;

            var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";

            lock (_lock)
            {
                try
                {
                    File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
                }
                catch
                {
                    // Silently fail if logging fails - don't want to crash the app
                }
            }
        }

        /// <summary>
        /// Gets the path to the current log file.
        /// </summary>
        public string LogFilePath => _logFilePath;
    }
}
