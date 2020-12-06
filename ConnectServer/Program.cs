using System;
using DatabaseClient;
using Servers;
using Toolbelt;
using static Toolbelt.Logger;
using Toolbelt;

namespace ConnectServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger.SetLoggingLevel(LOGGINGLEVEL.ALL, "ConnectServer.log");

            ConfigHandler.ReadConfigs();

            SessionHandler.Initialize();
            Logger.Info("Session Handler Initialized");

            //@TODO: change how configurations are loaded
            AuthServer.Initialize(ConfigHandler.LoginConfig.LoginAuthIP, ConfigHandler.LoginConfig.LoginAuthPort);
            Logger.Info("Auth Server Initialized");
            ViewServer.Initialize(ConfigHandler.LoginConfig.LoginViewIP, ConfigHandler.LoginConfig.LoginViewPort);
            Logger.Info("View Server Initialized");
            DataServer.Initialize(ConfigHandler.LoginConfig.LoginDataIP, ConfigHandler.LoginConfig.LoginDataPort);
            Logger.Info("Data Server Initialized");

            Logger.SetLoggingLevel(Logger.LOGGINGLEVEL.ALL, "ConnectServer.log");

            DBClient.TestSerialize();
            Console.ReadKey();
        }
    }
}
