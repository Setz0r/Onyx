using Ansi;
using static Ansi.AnsiFormatter;
using System;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace Toolbelt
{
    public static class Logger
    {
        [Flags]
        public enum LOGGINGLEVEL : byte
        {
            NONE     = 0x00,
            NORMAL   = 0x01,
            INFO     = 0x02,
            SUCCESS  = 0x04,
            WARNING  = 0x08,
            ERROR    = 0x10,

            BASIC    = NORMAL | INFO | SUCCESS,
            PROBLEMS = WARNING | ERROR,

            ALL      = BASIC | PROBLEMS
        }

        private static LOGGINGLEVEL _logginglevel = 0;

        private static bool LoggingToFile = false;
        private static string LogFileLocation = "";

        public static void SetLoggingLevel(LOGGINGLEVEL loggingLevel, string LogFile = "")
        {
            Ansi.WindowsConsole.TryEnableVirtualTerminalProcessing();

            _logginglevel = loggingLevel;
            if (!string.IsNullOrEmpty(LogFile))
            {
                LoggingToFile = true;
                LogFileLocation = "Logs/" + LogFile;
            }
        }

        public static void WriteToLogFile(string log)
        {
            using (StreamWriter w = File.AppendText(LogFileLocation))
            {
                w.WriteLine(log);
            }
        }

        public static void Display(string logText, object[] args = null)
        {
            string outString = string.Empty;
            if (args == null)
                outString = string.Format(Colorize($"{ConsoleColor.DarkGray}") + DateTime.Now.ToString("[yyyy/MM/dd HH:mm:ss.ff]") + logText);
            else
                outString = string.Format(Colorize($"{ConsoleColor.DarkGray}") + DateTime.Now.ToString("[yyyy/MM/dd HH:mm:ss.ff]") + logText, args.Select(x => x.ToString()).ToArray());
            Console.WriteLine(outString);
            Console.Write(new StringBuilder().SetMode(Mode.Reset));
            string FileText = Regex.Replace(outString, @"\x1b\[([0-9,A-Z]{1,2}(;[0-9]{1,2})?(;[0-9]{3})?)?[m|K]?", "");
            if (LoggingToFile)
                WriteToLogFile(FileText);
        }

        public static void Write(string logText, object[] args = null)
        {           
            if ((_logginglevel & LOGGINGLEVEL.NORMAL) > 0)
                Display(Colorize($"{ ConsoleColor.White}[LOG] ") + logText, args);
        }

        public static void Success(string logText, object[] args = null)
        {
            if ((_logginglevel & LOGGINGLEVEL.SUCCESS) > 0)
                Display(Colorize($"{ ConsoleColor.Green}[SUCCESS]{ConsoleColor.White} ") + logText, args);
        }

        public static void Info(string logText, object[] args = null)
        {
            if ((_logginglevel & LOGGINGLEVEL.INFO) > 0)
                Display(Colorize($"{ ConsoleColor.Cyan}[INFO]{ConsoleColor.White} ") + logText, args);
        }

        public static void Warning(string logText, object[] args = null)
        {
            if ((_logginglevel & LOGGINGLEVEL.WARNING) > 0)
                Display(Colorize($"{ ConsoleColor.Yellow}[WARNING]{ConsoleColor.White} ") + logText, args);
        }

        public static void Error(string logText, object[] args = null)
        {
            if ((_logginglevel & LOGGINGLEVEL.ERROR) > 0)
                Display(Colorize($"{ ConsoleColor.Red}[ERROR]{ConsoleColor.White} ") + logText, args);
        }
    }
}
