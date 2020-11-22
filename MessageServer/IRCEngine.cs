using System.Threading;
using IrcD.Core;
using IrcD.Core.Utils;

namespace MessageServer
{
    class IRCEngine
    {
        private static bool blocking = false;


        public void Run()
        {
            var settings = new IRCSettings();
            var ircDaemon = new IrcDaemon(settings.GetIrcMode());
            settings.SetDaemon(ircDaemon);
            settings.LoadSettings();

            if (blocking)
            {
                ircDaemon.Start();
            }
            else
            {
                ircDaemon.ServerRehash += ServerRehash;

                var serverThread = new Thread(ircDaemon.Start)
                {
                    IsBackground = false,
                    Name = "serverThread-1"
                };

                serverThread.Start();
            }
        }

        static void ServerRehash(object sender, RehashEventArgs e)
        {
            var settings = new IRCSettings();
            settings.SetDaemon(e.IrcDaemon);
            settings.LoadSettings();
        }
    }
}
