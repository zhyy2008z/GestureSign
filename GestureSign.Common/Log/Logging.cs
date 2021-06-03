using System;
using System.IO;
using System.Linq;
using System.Text;

using GestureSign.Common.Configuration;

namespace GestureSign.Common.Log
{
    public class Logging
    {
        private static string _logFilePath;

        private class StreamWriterWithTimestamp : TextWriter
        {
            public override Encoding Encoding => Encoding.UTF8;

            private string GetTimestamp()
            {
                return "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] ";
            }

            private string GetVersion()
            {
                return "[" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version + "] ";
            }

            public override void WriteLine(string value)
            {
                var line = GetTimestamp() + GetVersion() + value;
                File.AppendAllLines(LogFilePath, Enumerable.Repeat(line, 1));
            }

            public override void Write(string value)
            {
                var text = GetTimestamp() + GetVersion() + value;
                File.AppendAllText(LogFilePath, text);
            }
        }

        public static string LogFilePath => _logFilePath;

        public static bool OpenLogFile()
        {
            bool result;
            try
            {
                _logFilePath = Path.Combine(AppConfig.ApplicationDataPath, "GestureSign.log");
                CheckLogSize(_logFilePath);
                var sw = new StreamWriterWithTimestamp();
                Console.SetOut(sw);
                Console.SetError(sw);
                result = true;
            }
            catch (IOException e)
            {
                Console.WriteLine(e.ToString());
                result = false;
            }
            return result;
        }

        public static void LogException(Exception e)
        {
            if (!(e is ObjectDisposedException))
            {
                Console.WriteLine(e);
                Console.WriteLine();
                if (e.InnerException != null)
                    LogException(e.InnerException);
            }
        }

        public static void LogMessage(string message)
        {
            Console.WriteLine(message);
            Console.WriteLine();
        }

        private static void CheckLogSize(string logPath)
        {
            if (File.Exists(logPath))
            {
                if (new FileInfo(logPath).Length > 10240)
                    File.Delete(logPath);
            }
        }
    }
}
