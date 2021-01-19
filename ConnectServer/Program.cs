using System;
using DatabaseClient;
using Servers;
using Toolbelt;
using static Toolbelt.Logger;

namespace ConnectServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger.SetLoggingLevel(LOGGINGLEVEL.ALL, "ConnectServer.log");

            SessionHandler.Initialize();
            Logger.Info("Session Handler Initialized");

            // TODO: change how configurations are loaded

            Logger.SetLoggingLevel(Logger.LOGGINGLEVEL.ALL, "ConnectServer.log");

            DBClient.TestSerialize();
            Console.ReadKey();
        }
    }
}
