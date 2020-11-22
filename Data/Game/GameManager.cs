using Data.World;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Toolbelt;

namespace Data.Game
{
    public class GameManager
    {
        public Zone[] Zones;

        public void Initialize()
        {            
            Logger.Info("Initializing Game Manager");
            // @todo: initialize here
            Logger.Info("Game Manager Initialized, Ready to Rock!");
        }

        public void GameLoop()
        {
            bool active = true;
            Logger.Info("Entering Game Loop");
            while(active)
            {
                if (Console.KeyAvailable)
                {
                    active = false;
                    Logger.Info("Leaving Game Loop");
                }
                Thread.Sleep(1); // Prevent CPU overload
            }
        }

        public void Shutdown()
        {
            Logger.Info("Shutting Down Game Manager");
        }

    }
}
