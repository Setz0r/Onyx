using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Data.Game;
using DatabaseClient;
using MongoDB.Bson;
using Servers;
using Toolbelt;
using static Toolbelt.Logger;

namespace ConnectServer
{

    public static class ThreadExtension
    {
        public static void WaitAll(this IEnumerable<Thread> threads)
        {
            if (threads != null)
            {
                foreach (Thread thread in threads)
                { thread.Join(); }
            }
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            Logger.SetLoggingLevel(LOGGINGLEVEL.ALL);

            //ConfigHandler.ReadConfigs();

            SessionHandler.Initialize();
            Logger.Info("Session Handler Initialized");

            //// TODO: change how configurations are loaded
            
            AuthServer.Initialize("127.0.0.1", 54231);
            Logger.Info("Auth Server Initialized");
            ViewServer.Initialize("127.0.0.1", 54001);
            Logger.Info("View Server Initialized");
            DataServer.Initialize("127.0.0.1", 54230);
            Logger.Info("Data Server Initialized");

            //long updated = DBClient.UpdateOne<Account>(DBREQUESTTYPE.ACCOUNT,a => a.AccountId == 1001,new Dictionary<string, object>() { {"PlayerVars.Test", 5 } } );
            //Console.WriteLine(updated);
        }
    }
}
