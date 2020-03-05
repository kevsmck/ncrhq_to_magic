using System;

namespace NCRHQ_to_Magic.Util
{
    class Logging
    {
        public enum Type
        {
            None,
            Debug,
            Info,
            Warning,
            Error,
            Critical
        }

        /// Log(Logging.Type, string, string, string)
        /// <summary>
        /// Provides logging for various reasons
        /// </summary>
        /// <param name="type"></param>
        /// <param name="summary"></param>
        /// <param name="logCode"></param>
        /// <param name="detail"></param>
        public static void Log(Type type, string summary, string logCode, string detail)
        {
            // TODO: Implement additional logging - possibly EventLog
            //       And export to a file for Error/Critical
            string consoleOutput = string.Empty;
            switch (type)
            {
                case Type.None:
                    consoleOutput = string.Empty;
                    break;
                case Type.Debug:
                    consoleOutput = "[DEBUG] ";
                    break;
                case Type.Info:
                    consoleOutput = "[INFO] ";
                    break;
                case Type.Warning:
                    consoleOutput = "[WARNING] ";
                    break;
                case Type.Error:
                    consoleOutput = "[ERROR] ";
                    break;
                case Type.Critical:
                    consoleOutput = "[CRITICAL] ";
                    break;
                default:
                    consoleOutput = "[???] ";
                    break;
            }
            if (!type.Equals(Type.None))
            {
                consoleOutput = string.Concat(consoleOutput, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                if (!string.IsNullOrEmpty(logCode))
                {
                    consoleOutput = string.Concat(consoleOutput, " - ", logCode);
                }
                if (!string.IsNullOrEmpty(summary))
                {
                    consoleOutput = string.Concat(consoleOutput, " - ", summary);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(summary))
                {
                    consoleOutput = string.Concat(consoleOutput, summary);
                }
            }
            if (!string.IsNullOrEmpty(detail))
            {
                consoleOutput = string.Concat(consoleOutput, " - Detail: ", detail);
            }

            // Output the error to the console
            Console.WriteLine(consoleOutput);
        }
    }
}
