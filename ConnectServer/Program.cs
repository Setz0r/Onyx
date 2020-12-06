using System;
using DatabaseClient;
using Toolbelt;

namespace ConnectServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger.SetLoggingLevel(Logger.LOGGINGLEVEL.ALL, "ConnectServer.log");
            DBClient.TestSerialize();
            Console.ReadKey();
        }
    }
}
