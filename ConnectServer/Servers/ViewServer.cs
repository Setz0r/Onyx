using Networking;
using System;
using System.Net;
using System.Security.Cryptography;
using Toolbelt;
using static Ansi.AnsiFormatter;
using System.Text.RegularExpressions;
using ConnectServer;
using DatabaseClient;
using Data.Game.Entities;
using Data.Game;
using Data.OnyxMath;

namespace Servers
{
    public static class ViewServer
    {
        public static byte[] wdata;
        public static TCPServer viewServer;
        
        public static byte[] lobbyErrorData =
        {
            0x24, 0x00, 0x00, 0x00, 0x49, 0x58, 0x46, 0x46, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00
        };

        public static byte[] lobbyActionDone =
        {
            0x20, 0x00, 0x00, 0x00, 0x49, 0x58, 0x46, 0x46, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        };


        private static int ViewConnectHandler(SessionTcpClient client)
        {
            var addr = ((IPEndPoint)client.Client.RemoteEndPoint).Address;
            var port = ((IPEndPoint)client.Client.RemoteEndPoint).Port;
            Logger.Info("View Server Connection : {0} : {1}", new object[] { addr.ToString(), port });
            return 1;
        }

        private static bool CreateCharacter(uint accountID, string charName, ByteRef buf)
        {
            Random rnd = new Random();

            Player player = new Player()
            {
                AccountId = accountID,
                TimeCreate = Utility.Timestamp(),
                TimeLastModify = Utility.Timestamp()
            };

            // TODO: put all these class initializers in various constructors
            player.Name = charName;
            player.Look = new Data.Game.Entities.Look();
            player.Look.Model = new ModelInfo();
            player.Look.Model.Race = buf.GetByte(48);
            player.Look.Size = buf.GetByte(57);            
            player.Look.Model.Face = buf.GetByte(60);
            
            player.Stats = new PlayerStats();
            
            byte job = buf.GetByte(50);

            player.Stats.Job = (byte)Utility.Clamp(job, 1, 6);
            
            if (player.Stats.Job != job)
            {
                Logger.Warning("{0} tried to create a character with an invalid starting job", new object[] { charName });
            }
            player.Profile = new ProfileInfo();
            player.Profile.Nation = buf.GetByte(54);
            player.Location = new LocationInfo();
            player.Location.Position = new PositionInfo();
            player.Location.Position.Pos = new OnyxVec3(0,0,0);
            switch(player.Profile.Nation)
            {
                case 0x02:
                    while (player.Location.CurrentZone == 0 || player.Location.CurrentZone == 0xEF)
                    {
                        player.Location.CurrentZone = (ushort)rnd.Next(0xEE, 0xF1);
                    }
                    break;
                case 0x01:
                    player.Location.CurrentZone = (ushort)rnd.Next(0xEA, 0xEC);
                    break;
                case 0x00:
                    player.Location.CurrentZone = (ushort)rnd.Next(0xE6, 0xE8);
                    break;
            }

            uint PlayerID = 1000;
            uint maxPlayerID = DBClient.GetMaxID(DBREQUESTTYPE.PLAYER);

            if (maxPlayerID > 0)
                PlayerID = maxPlayerID + 1;

            player.PlayerId = PlayerID;

            bool success = DBClient.InsertOne<Player>(DBREQUESTTYPE.PLAYER, player);
            if (success)
            {
                Logger.Success("New Character Created : {0}", new object[] { charName });
            }
            else
            {
                Logger.Error("Failed to Create Character : {0}", new object[] { charName });
            }

            return success;
        }

        private static int ViewDataHandler(SessionTcpClient client, byte[] data, int Length)
        {
            var addr = ((IPEndPoint)client.Client.RemoteEndPoint).Address;
            ByteRef recv = new ByteRef(data, Length);

            byte id = recv.GetByte(8);
            int sendSize = 0x28;
            ByteRef reserveData = new ByteRef(sendSize);
            //Logger.Log("View Server Data Handler");
            //Logger.Log(Utility.ByteArrayToString(data));
            //Logger.Log("VIEW ID {0:X}", id);
            switch (id)
            {
                case 0x26:
                    ByteRef SessionHash = new ByteRef(16);
                    SessionHash.BlockCopy(recv.At(12), 0, 16);
                    LoginSession session = SessionHandler.GetSessionByHash(addr.ToString(), SessionHash.Get());
                    if (session != null)
                    {
                        client.Session = session;
                        session.View_client = client;
                        session.Status = SESSIONSTATUS.CHARSELECT;

                        string clientVersionStr = recv.GetString(0x74, 6);
                        string expectedVersionStr = "301812";//ConfigHandler.VersionConfig.ClientVersion.Substring(0, 6);

                        uint clientVersion = Convert.ToUInt32(clientVersionStr);
                        uint expectedVersion = Convert.ToUInt32(expectedVersionStr);

                        bool versionMismatch = clientVersion != expectedVersion;

                        bool fatalMismatch = false;

                        if (versionMismatch)
                        {
                            switch (ConfigHandler.VersionConfig.VersionLock)
                            {
                                case 1:
                                    if (expectedVersion < clientVersion)
                                    {
                                        fatalMismatch = true;
                                    }
                                    break;
                                case 2:
                                    if (expectedVersion > clientVersion)
                                    {
                                        fatalMismatch = true;
                                    }
                                    break;
                            }
                        }

                        if (fatalMismatch)
                        {
                            sendSize = 0x24;
                            reserveData.Reset(0x24);
                            reserveData.BlockCopy(lobbyErrorData, 0, sendSize);
                            reserveData.Set<ushort>(0x20, 331);
                        }
                        else
                        {
                            // TODO: get expansions and features from database
                            AccountEF aef = new AccountEF() {  Expansions = 14, Features = 13 }; // MySQL.GetAccountEF(client.Session.Account_id);
                            ByteRef response = new ByteRef(new byte[] {
                                0x28, 0x00, 0x00, 0x00, 0x49, 0x58, 0x46, 0x46, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x4f, 0xe0, 0x5d, 0xad,
                                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
                            });

                            response.Set<ushort>(32, aef.Expansions); // expansion bitmask
                            response.Set<ushort>(36, aef.Features);   // feature bitmask
                            reserveData.BlockCopy(response.Get(), 0, sendSize);
                        }

                        MD5 md5Hash = MD5.Create();
                        byte[] hash = md5Hash.ComputeHash(reserveData.Get(), 0, sendSize);
                        reserveData.BlockCopy(hash, 12, 16);

                        client.Session.ViewSend(reserveData.Get());
                    }
                    else
                    {
                        sendSize = 0x24;
                        reserveData.Reset(0x24);
                        reserveData.BlockCopy(lobbyErrorData, 0, sendSize);
                        reserveData.Set<ushort>(0x20, 314);
                        MD5 md5Hash = MD5.Create();
                        byte[] hash = md5Hash.ComputeHash(reserveData.Get(), 0, sendSize);
                        reserveData.BlockCopy(hash, 12, 16);
                        client.Client.Send(reserveData.Get());
                    }


                    break;

                case 0x1F:
                    reserveData.Reset(5);
                    reserveData.Set<byte>(0, 0x01);

                    client.Session.DataSend(reserveData.Get());
                    break;

                case 0x24:
                    byte[] Data24 = new byte[]
                    {
                        0x40, 0x00, 0x00, 0x00, 0x49, 0x58, 0x46, 0x46, 0x23, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00,
                        0x64, 0x00, 0x00, 0x00, 0x70, 0x58, 0x49, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
                    };

                    ByteRef ReservePacket24 = new ByteRef(Data24);

                    // TODO: bring in from configuration
                    ReservePacket24.Set<string>(36, "Onyx");

                    MD5 md5Hash24 = MD5.Create();
                    byte[] hash24 = md5Hash24.ComputeHash(ReservePacket24.Get(), 0, 64);
                    ReservePacket24.BlockCopy(hash24, 12, 16);

                    client.Session.ViewSend(ReservePacket24.Get(), 64);
                    break;

                case 0x07:
                    reserveData.Reset(5);
                    reserveData.Set<byte>(0, 0x02);
                    if (recv.Length > 30)
                    {
                        client.Session.Char_id = recv.GetUInt32(28);
                    }
                    else
                    {
                        SessionHandler.KillSession(client.Session);
                        return -1;
                    }
                    client.Session.DataSend(reserveData.Get());
                    break;

                case 0x14: // Delete Character
                    byte[] Data14 = new byte[]
                    {
                        0x20, 0x00, 0x00, 0x00, 0x49, 0x58, 0x46, 0x46, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
                    };

                    uint charid = recv.GetUInt32(0x20);
                    if (charid > 0 && client.Session.Char_id_list.Contains(charid))
                    {
                        ByteRef ReservePacket14 = new ByteRef(Data14);

                        MD5 md5Hash14 = MD5.Create();
                        byte[] hash14 = md5Hash14.ComputeHash(ReservePacket14.Get(), 0, 0x20);
                        ReservePacket14.BlockCopy(hash14, 12, 16);

                        client.Session.ViewSend(ReservePacket14.Get());

                        // TODO: add character deletion
                        //MySQL.DeleteChar(client.Session.Account_id, charid);
                    }
                    else
                    {
                        SessionHandler.KillSession(client.Session);
                        return -1;
                    }
                    break;

                case 0x21: // Create a Character - Save
                    if (!CreateCharacter(client.Session.Account_id, client.Session.Char_name, recv))
                    {
                        SessionHandler.KillSession(client.Session);
                        return -1;
                    }

                    ByteRef ReservePacket21 = new ByteRef(0x20);
                    ReservePacket21.BlockCopy(lobbyActionDone, 0, 0x20);
                    MD5 md5Hash21 = MD5.Create();
                    byte[] hash21 = md5Hash21.ComputeHash(ReservePacket21.Get(), 0, 0x20);
                    ReservePacket21.BlockCopy(hash21, 12, 16);

                    client.Session.ViewSend(ReservePacket21.Get());

                    client.Session.Status = SESSIONSTATUS.INGAME;
                    break;

                case 0x22: // Create a Character - Validate
                    sendSize = 0x24;
                    ByteRef ReservePacket22 = new ByteRef(sendSize);
                    if (ConfigHandler.MaintConfig.MaintMode > 0)
                    {
                        ReservePacket22.BlockCopy(lobbyErrorData, 0, 0x24);
                        ReservePacket22.Set<ushort>(32, 314);
                    }
                    else
                    {
                        ByteRef NameBuf = new ByteRef(15);
                        NameBuf.BlockCopy(recv.At(32), 0, 15);                        
                        string CharName = Utility.ReadCString(NameBuf.Get());

                        Player player = DBClient.GetOne<Player>(DBREQUESTTYPE.PLAYER, p => p.Name.Equals(CharName));

                        if (!Regex.IsMatch(CharName, @"^[a-zA-Z]+$") || SessionHandler.CharNameExists(CharName) || player != null)
                        {
                            ReservePacket22.BlockCopy(lobbyErrorData, 0, sendSize);
                            ReservePacket22.Set<ushort>(32, 313);
                        }
                        else
                        {
                            client.Session.Char_name = CharName;
                            sendSize = 0x20;
                            ReservePacket22.BlockCopy(lobbyActionDone, 0, sendSize);
                        }
                    }

                    MD5 md5Hash22 = MD5.Create();
                    byte[] hash22 = md5Hash22.ComputeHash(ReservePacket22.Get(), 0, 0x20);
                    ReservePacket22.BlockCopy(hash22, 12, 16);

                    client.Session.ViewSend(ReservePacket22.Get(),sendSize);
                    break;
            }

            return 1;
        }

        private static int ViewDisconnectHandler(SessionTcpClient client)
        {
            if (!client.IsDead)
            {
                var addr = ((IPEndPoint)client.Client.RemoteEndPoint).Address;
                var port = ((IPEndPoint)client.Client.RemoteEndPoint).Port;
                Logger.Info("View Server Disconnection : {0} : {1}", new object[] { addr.ToString(), port });
            }
            SessionHandler.CheckSessionHealth(client.Session);
            return 1;
        }

        public static void Initialize(string address, int port)
        {
            viewServer = new TCPServer("View Server", address, port, ViewConnectHandler, ViewDataHandler, ViewDisconnectHandler);
        }
    }
}
