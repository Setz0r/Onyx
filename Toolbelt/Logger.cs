using Ansi;
using static Ansi.AnsiFormatter;
using System;
using System.Linq;
using System.Text;

namespace Toolbelt
{
    public static class Logger
    {
        public enum LOGGINGLEVEL
        {
            NONE    = 0,
            NORMAL  = 1,
            INFO    = 2,
            SUCCESS = 3,
            WARNING = 4,
            ERROR   = 5
        }

        private static LOGGINGLEVEL _logginglevel = 0;

        public static void SetLoggingLevel(LOGGINGLEVEL loggingLevel)
        {
            _logginglevel = loggingLevel;
        }

        public static void Display(string logText, object[] args = null)
        {
            string outString = String.Empty;
            if (args == null)
                outString = String.Format(Colorize($"{ConsoleColor.DarkGray}") + DateTime.Now.ToString("[yyyy/MM/dd HH:mm:ss.ff]") + logText);
            else
                outString = String.Format(Colorize($"{ConsoleColor.DarkGray}") + DateTime.Now.ToString("[yyyy/MM/dd HH:mm:ss.ff]") + logText, args.Select(x => x.ToString()).ToArray());
            Console.WriteLine(outString);
            Console.Write(new StringBuilder().SetMode(Mode.Reset));
        }

        public static void Write(string logText, object[] args = null)
        {           
            if (_logginglevel >= LOGGINGLEVEL.NORMAL)
                Display(Colorize($"{ ConsoleColor.White}[LOG] ") + logText, args);
        }

        public static void Success(string logText, object[] args = null)
        {
            if (_logginglevel >= LOGGINGLEVEL.SUCCESS)
                Display(Colorize($"{ ConsoleColor.Green}[SUCCESS]{ConsoleColor.White} ") + logText, args);
        }

        public static void Info(string logText, object[] args = null)
        {
            if (_logginglevel >= LOGGINGLEVEL.INFO)
                Display(Colorize($"{ ConsoleColor.Cyan}[INFO]{ConsoleColor.White} ") + logText, args);
        }

        public static void Warning(string logText, object[] args = null)
        {
            if (_logginglevel >= LOGGINGLEVEL.WARNING)
                Display(Colorize($"{ ConsoleColor.Yellow}[WARNING]{ConsoleColor.White} ") + logText, args);
        }

        public static void Error(string logText, object[] args = null)
        {
            if (_logginglevel >= LOGGINGLEVEL.ERROR)
                Display(Colorize($"{ ConsoleColor.Red}[ERROR]{ConsoleColor.White} ") + logText, args);
        }
    }
}
