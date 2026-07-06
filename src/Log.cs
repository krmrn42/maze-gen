using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace PlayersWorlds.Maps {
    /// <summary>
    /// A quick implementation of logging for inside use.
    /// </summary>
    public class Log {
        /// <summary>
        /// Creates a new instance of the <see cref="Log"/> class with console
        /// output.
        /// </summary>
        public static Log ToConsole<T>() {
            return new Log(typeof(T).Name, Console.Out);
        }
        /// <summary>
        /// Creates a new instance of the <see cref="Log"/> class with console
        /// output.
        /// </summary>
        public static Log ToConsole(string name) {
            return new Log(name, Console.Out);
        }

        private readonly string _name;

        // Output stream for log messages
        private readonly TextWriter _output;

        /// <summary>
        /// Debug messages at this or lower level will be logged. 
        /// </summary>
        public static int DebugLoggingLevel { get; set; } = 0;

        // Dictionary to track messages and their counts
        private readonly Dictionary<string, int> _messageLogCounts =
            new Dictionary<string, int>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="output"></param>
        public Log(string name, TextWriter output) {
            _name = name;
            _output = output;
        }

        /// <summary>
        /// I: Log at Info level (always logged)
        /// </summary>
        /// <param name="message"></param>
        public void I(object message) {
            Write(0, message);
        }

        /// <summary>
        /// I: Log at Info level (every <paramref name="logEvery" /> message is
        /// always logged)
        /// </summary>
        /// <param name="logEvery"></param>
        /// <param name="message"></param>
        public void I(int logEvery, object message) {
            WriteEvery(logEvery, 0, message);
        }

        /// <summary>
        /// D: Log at a given debug level
        /// </summary>
        /// <param name="debugLevel"></param>
        /// <param name="message"></param>
        public void D(int debugLevel, object message) {
            if (debugLevel <= DebugLoggingLevel) {
                Write(debugLevel, message);
            }
        }

        /// <summary>
        /// D: Log at a given debug level (every <paramref name="logEvery" />
        /// message)
        /// </summary>
        /// <param name="debugLevel"></param>
        /// <param name="logEvery"></param>
        /// <param name="message"></param>
        public void D(int debugLevel, int logEvery, object message) {
            if (debugLevel <= DebugLoggingLevel) {
                WriteEvery(logEvery, debugLevel, message);
            }
        }

        private void WriteEvery(int logEvery,
                              int level,
                              object message,
                              [CallerFilePath] string filePath = "",
                              [CallerLineNumber] int lineNumber = 0) {
            var key = $"{filePath}:{lineNumber}";


            if (_messageLogCounts.TryGetValue(key, out var count)) {
                _messageLogCounts[key] = ++count;

                if (count % logEvery == 0) {
                    Write(level, $"(x{count}) " + message);
                }
            } else {
                // Always write the first message
                _messageLogCounts.Add(key, 1);
                Write(level, message);
            }
        }

        // Common log method
        private void Write(int level, object message) {
            _output.WriteLine($"[{DateTime.Now}] [{_name}-{level}] {message}");
        }
    }
}
