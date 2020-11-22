using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Networking
{
    public class IRCClient
    {
        private const string server = "irc.freenode.net";
        private const string gecos = "CerkitBot v1.0 (cerkit.com)";
        private const string nick = "YourBotNick";
        private const string password = "YOUR_IRC_PASSWORD";
        private const string ident = "cerkitbot";
        private const string channel = "#CHANNEL_NAME";

        public void Test(string[] args)
        {
            using (var client = new TcpClient())
            {
                Console.WriteLine($"Connecting to {server}");
                client.Connect(server, 6667);
                Console.WriteLine($"Connected: {client.Connected}");

                using (var stream = client.GetStream())
                using (var writer = new StreamWriter(stream))
                using (var reader = new StreamReader(stream))
                {
                    writer.WriteLine($"USER {ident} * 8 {gecos}");
                    writer.WriteLine($"NICK {nick}");
                    // identify with the server so your bot can be an op on the channel
                    writer.WriteLine($"PRIVMSG NickServ :IDENTIFY {nick} {password}");
                    writer.Flush();

                    while (client.Connected)
                    {
                        var data = reader.ReadLine();

                        if (data != null)
                        {
                            var d = data.Split(' ');
                            Console.WriteLine($"Data: {data}");

                            if (d[0] == "PING")
                            {
                                writer.WriteLine("PONG");
                                writer.Flush();
                            }

                            if (d.Length > 1)
                            {

                                switch (d[1])
                                {
                                    case "376":
                                    case "422":
                                        {
                                            writer.WriteLine($"JOIN {channel}");

                                            // communicate with everyone on the channel as soon as the bot logs in
                                            writer.WriteLine($"PRIVMSG {channel} :Hello, World!");
                                            writer.Flush();
                                            break;
                                        }
                                    case "PRIVMSG":
                                        {
                                            if (d.Length > 2)
                                            {
                                                if (d[2] == nick)
                                                {
                                                    // someone sent a private message to the bot
                                                    var sender = data.Split('!')[0].Substring(1);
                                                    var message = data.Split(':')[2];
                                                    Console.WriteLine($"Message: {message}");
                                                    // handle all your bot logic here
                                                    writer.WriteLine($@"PRIVMSG {sender} :Hello, thank you for talking to me.");
                                                    writer.Flush();
                                                }
                                            }
                                            break;
                                        }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
