using System;
using System.Collections.Generic;
using UnityEngine;

namespace STSLite.Core.Logging
{
    public class Logger
    {
        private static readonly object LockObj = new object();
        private static readonly ILogPrinter LogPrinter = new UnityLogPrinter();

        private readonly LogType _logType;

        public static LogLevel GlobalLogLevel { get; set; } = LogLevel.Info;

        public static readonly Dictionary<LogType, LogLevel> logLevelTypeMap = new Dictionary<LogType, LogLevel>
        {
            { LogType.Network, LogLevel.Info },
            { LogType.Actions, LogLevel.Info },
            { LogType.Generic, LogLevel.Info },
            { LogType.GameSync, LogLevel.Info },
            { LogType.VisualSync, LogLevel.Info },
        };

        public string? Context { get; set; }

        public event Action<LogLevel, string, int>? LogCallback;
        public static event Action<LogLevel, LogType, string, int>? GlobalLogCallback;

        static Logger()
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 2; i++)
            {
                if (args[i] != "-log")
                {
                    continue;
                }

                if (!Enum.TryParse(args[i + 1], ignoreCase: true, out LogType logType))
                {
                    LogPrinter.Print(LogLevel.Error, $"Invalid log type: {args[i + 1]}", 1);
                    continue;
                }

                if (!Enum.TryParse(args[i + 2], ignoreCase: true, out LogLevel logLevel))
                {
                    LogPrinter.Print(LogLevel.Error, $"Invalid log level: {args[i + 2]}", 1);
                    continue;
                }

                logLevelTypeMap[logType] = logLevel;
                LogPrinter.Print(LogLevel.Info, $"Log level for {logType} set to {logLevel}", 1);
            }
        }

        public Logger(string? context = null, LogType logType = LogType.Generic)
        {
            Context = context;
            _logType = logType;
        }

        public bool WillLog(LogLevel level)
        {
            LogLevel threshold = logLevelTypeMap.TryGetValue(_logType, out LogLevel configured)
                ? configured
                : GlobalLogLevel;
            return level >= threshold;
        }

        public void LogMessage(LogLevel level, string text, int skipFrames = 1)
        {
            string message = Context != null ? $"[{Context}] {text}" : text;
            LogMessage(level, _logType, message, skipFrames + 1);
        }

        public void LogMessage(LogLevel level, LogType type, string text, int skipFrames = 1)
        {
            if (!WillLog(level))
            {
                return;
            }

            lock (LockObj)
            {
                LogPrinter.Print(level, text, skipFrames + 1);
                LogCallback?.Invoke(level, text, skipFrames + 1);
                GlobalLogCallback?.Invoke(level, type, text, skipFrames + 1);
            }
        }

        public void Load(string text, int skipFrames = 1)
        {
            LogMessage(LogLevel.Load, text, skipFrames);
        }

        public void Debug(string text, int skipFrames = 1)
        {
            LogMessage(LogLevel.Debug, text, skipFrames);
        }

        public void VeryDebug(string text, int skipFrames = 1)
        {
            LogMessage(LogLevel.VeryDebug, text, skipFrames);
        }

        public void Info(string text, int skipFrames = 1)
        {
            LogMessage(LogLevel.Info, text, skipFrames);
        }

        public void Warn(string text, int skipFrames = 1)
        {
            LogMessage(LogLevel.Warn, text, skipFrames);
        }

        public void Error(string text, int skipFrames = 1)
        {
            LogMessage(LogLevel.Error, text, skipFrames);
        }

        public static void SetLogLevelForType(LogType type, LogLevel? logLevel)
        {
            if (logLevel.HasValue)
            {
                logLevelTypeMap[type] = logLevel.Value;
            }
            else
            {
                logLevelTypeMap.Remove(type);
            }
        }
    }
}