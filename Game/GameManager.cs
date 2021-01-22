using Data.Game;
using Data.World;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Toolbelt;

namespace Game
{
    public class GameManager
    {
        public List<ZoneCluster> clusters;
        public List<Task> clusterTasks;

        public GameManager()
        {
            clusters = new List<ZoneCluster>();
        }

        public void LoadZoneClusters()
        {
            // TODO: replace with yaml loading
            ZoneCluster clusterA = new ZoneCluster();            
            Zone testZoneA = new Zone(ZONEID.BASTOK_MARKETS);
            clusterA.zones.TryAdd(ZONEID.BASTOK_MARKETS, testZoneA);
            Zone testZoneB = new Zone(ZONEID.BASTOK_MINES);
            clusterA.zones.TryAdd(ZONEID.BASTOK_MINES, testZoneB);
            Zone testZoneC = new Zone(ZONEID.PORT_BASTOK);
            clusterA.zones.TryAdd(ZONEID.PORT_BASTOK, testZoneC);
            Zone testZoneD = new Zone(ZONEID.METALWORKS);
            clusterA.zones.TryAdd(ZONEID.METALWORKS, testZoneD);
            clusters.Add(clusterA);
            ZoneCluster clusterB = new ZoneCluster();
            Zone testZoneE = new Zone(ZONEID.NORTH_GUSTABERG);
            clusterB.zones.TryAdd(ZONEID.NORTH_GUSTABERG, testZoneE);
            Zone testZoneF = new Zone(ZONEID.ZERUHN_MINES);
            clusterB.zones.TryAdd(ZONEID.ZERUHN_MINES, testZoneF);
            Zone testZoneG = new Zone(ZONEID.PALBOROUGH_MINES);
            clusterB.zones.TryAdd(ZONEID.PALBOROUGH_MINES, testZoneG);
            Zone testZoneH = new Zone(ZONEID.SOUTH_GUSTABERG);
            clusterB.zones.TryAdd(ZONEID.SOUTH_GUSTABERG, testZoneH);
            clusters.Add(clusterB);
        }

        public void Initialize()
        {            
            Logger.Info("Initializing Game Manager");
            
            //  TODO: initialize here
            LoadZoneClusters();
            clusterTasks = new List<Task>();

            foreach (var cluster in clusters)
            {   
                foreach (var zone in cluster.zones)
                {
                    zone.Value.Initialize();
                }
                Task task = new Task(cluster.ClusterLoop);
                clusterTasks.Add(task);
                task.Start();
            }

            Logger.Info("Game Manager Initialized, Ready to Rock!");
        }

        public void GameLoop()
        {
            bool active = true;
            uint portnum = 54240;

            Logger.Info("Entering Game Loop");
            Logger.Info("Setting Zones to Listen");

            foreach (var cluster in clusters)
            {
                cluster.Listen("127.0.0.1", portnum);
                portnum++;
            }

            while (active)
            {
                if (Console.KeyAvailable)
                {
                    active = false;
                    Logger.Info("Leaving Game Loop");
                }
                Thread.Sleep(1000); // Prevent CPU overload
                int number = Process.GetCurrentProcess().Threads.Count;
                //Logger.Info("Threads in Use: {0}", new object[] { number });
            }
        }

        public void Shutdown()
        {
            Logger.Info("Shutting Down Signal Received");
            Logger.Info("Sending Shutdown Signal to All Active Zones");

            foreach (var cluster in clusters)
            {
                cluster.status = CLUSTERSTATUS.SHUTTINGDOWN;
            }

            Task.WaitAll(clusterTasks.ToArray());

            Logger.Info("All Zones Shut Down Successfully");
            Logger.Info("Shutting Down Complete");
        }

    }
}
