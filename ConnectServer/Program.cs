using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Data.DataChunks.Incoming;
using Data.Game;
using Data.Game.Entities;
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

            string packetData = "53440300010200000002000003000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000";
            byte[] packetBytes = Utility.StringToByteArray(packetData);
            Player testPlayer = new Player();
            bool success = LockStyleInfo.Instance.Handler(testPlayer, packetBytes);


            VanaTime.TYPE t = VanaTime.GetInstance().Sync();
            uint d = VanaTime.GetInstance().VanaDate;
            uint month = VanaTime.GetInstance().Month;
            uint day = VanaTime.GetInstance().Day;
            uint year = VanaTime.GetInstance().Year;
            uint hour = VanaTime.GetInstance().Hour;
            uint minute = VanaTime.GetInstance().Minute;
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

            //ActiveSession ass = new ActiveSession()
            //{
            //    AccountID = 1234,
            //    PlayerID = 4321,
            //    EndPoint = "127.0.0.1:50301",
            //    CurrentZoneID = 245,
            //    NextZoneID = 245,
            //    Zoning = 0,
            //    SessionHash = "000000000000000000000000000000005CE05DAD"
            //};

            //DBClient.InsertOne(DBREQUESTTYPE.ACTIVESESSION, ass);

            //DBClient.UpdateOne<ActiveSession>(DBREQUESTTYPE.ACTIVESESSION, a => a.AccountID == 1234, new Dictionary<string, object> { { "CurrentZoneID", 206} });

            //DBClient.DeleteOne<ActiveSession>(DBREQUESTTYPE.ACTIVESESSION, a => a.AccountID == 1234);

            //long updated = DBClient.UpdateOne<Account>(DBREQUESTTYPE.ACCOUNT,a => a.AccountId == 1001,new Dictionary<string, object>() { {"PlayerVars.Test", 5 } } );
            //Console.WriteLine(updated);
        }
    }
}
