using Mabean.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Mabean.Services
{
    public static class LoggerService
    {
        private static StreamWriter? _writer;
        private static readonly object _lock = new();

        public static void Init()
        {
            if (_writer != null)
                throw new InvalidOperationException("Logger already initialized");


            _writer = new StreamWriter(
                new FileStream(Path.Combine(Paths.Logs, "log.txt"), FileMode.Append, FileAccess.Write, FileShare.Read))
            {
                AutoFlush = true
            };
        }

        public static void Write(string message)
        {
            if (_writer == null)
                throw new InvalidOperationException("Logger not initialized");

            lock (_lock)
            {
                _writer.WriteLine($"[{DateTime.UtcNow:O}] {message}");
            }
        }

        public static void Shutdown()
        {
            lock (_lock)
            {
                _writer?.Dispose();
                _writer = null;
            }
        }
    }
}
