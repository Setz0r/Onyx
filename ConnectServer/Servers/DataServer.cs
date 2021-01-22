using Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using Toolbelt;
using Data.Structs;
using System.Net;
using System.Security.Cryptography;
using static Ansi.AnsiFormatter;
using ConnectServer;
using Data.Game.Entities;
using DatabaseClient;

namespace Servers
{
    public static class DataServer
    {
        public static TCPServer dataServer;

        private static int DataConnectHandler(SessionTcpClient client)
        {
            var addr = ((IPEndPoint)client.Client.RemoteEndPoint).Address;
            var port = ((IPEndPoint)client.Client.RemoteEndPoint).Port;
            Logger.Info("Data Server Connection : {0} : {1}", new object[] {addr.ToString(), port });

            LoginSession session = SessionHandler.GetSessionByIP(addr.ToString(), SESSIONSTATUS.ACCEPTINGTERMS);

            if (session != null)
            {
                session.Data_client = client;
                session.Status = SESSIONSTATUS.MAINMENU;
                client.Session = session;
            }

            return 1;
        }

        private static void SendLobbyError(SessionTcpClient client)
        {
            MD5 md5Hash = MD5.Create();
            byte[] hash;
            ByteRef ReserveData = new ByteRef(ViewServer.lobbyErrorData);
            byte SendBuffSize = ReserveData.GetByte(0);

            ReserveData.Set<ushort>(32, 321);
            hash = md5Hash.ComputeHash(ReserveData.Get(), 0, SendBuffSize);
            ReserveData.BlockCopy(hash, 12, 16);
            client.Session.ViewSend(ReserveData.Get(), SendBuffSize);
            SessionHandler.KillSession(client.Session);
        }

        private static int DataDataHandler(SessionTcpClient client, byte[] data, int Length)
        {
            ByteRef recv = new ByteRef(data, Length);
            byte id = recv.GetByte(0);
            MD5 md5Hash;
            byte[] hash;
            //Logger.Log("Data Server Data Handler");
            //Logger.Log(Utility.ByteArrayToString(data));
            //Logger.Log("DATA ID {0:X}", id);
            switch (id)
            {
                case 0xA1:
                    if (data.Length < 9)
                    {
                        Logger.Warning("Data Server data size received incorrect.");
                        //SessionHandler.KillSession(client.Session);
                        return -1;
                    }

                    uint accountId = recv.GetUInt32(1);
                    if (accountId >= 1000)
                    {
                        ByteRef UList = new ByteRef(500);
                        ByteRef CharList = new ByteRef(2500);
                        ByteRef ReserveDataA1 = new ByteRef(new byte[] {
                            0xc4, 0x01, 0x00, 0x00, 0x49, 0x58, 0x46, 0x46, 0x20, 0x00, 0x00, 0x00, 0x2a, 0x72, 0x4a, 0x94,
                            0x4f, 0x60, 0x27, 0xc4, 0x45, 0x4b, 0x7d, 0xcf, 0x27, 0x8e, 0x6d, 0xcd, 0x03, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x56, 0x61, 0x6c, 0x65, 0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x05, 0x00,
                            0x07, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0e, 0x02, 0x00, 0x10, 0x00, 0x20, 0x00, 0x30,
                            0x00, 0x40, 0x00, 0x50, 0x00, 0x60, 0x00, 0x70, 0x00, 0x01, 0x01, 0x00, 0x02, 0x00, 0x00, 0x00,
                            0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x64, 0x00, 0x00, 0x00, 0x00, 0x00, 0xb5, 0xfa, 0x01, 0x00,
                            0x7e, 0x00, 0x00, 0x00, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01,
                            0x01, 0x01, 0x01, 0x01, 0x46, 0x6e, 0xcf, 0x09, 0xde, 0x17, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                            0x0a, 0x52, 0x03, 0x00, 0x0e, 0x08, 0x00, 0x00, 0x00, 0x0f, 0x00, 0x00
                        });

                        client.Session.Server_ip = recv.GetUInt32(5);

                        // Set reserved data
                        CharList.Set<byte>(0, 0xE0);
                        CharList.Set<byte>(1, 0x08);
                        CharList.Set<byte>(4, 0x49);
                        CharList.Set<byte>(5, 0x58);
                        CharList.Set<byte>(6, 0x46);
                        CharList.Set<byte>(7, 0x46);
                        CharList.Set<byte>(8, 0x20);

                        // Content IDS
                        // TODO: get number of content ids for account from database
                        byte ContentIDCount = 5; //MySQL.ContentIDCount(client.Session.Account_id);
                        CharList.Set<uint>(28, ContentIDCount);                     // content id count

                        // Server name in the lobby menu
                        // TODO: Get server name from config
                        ReserveDataA1.Set<string>(60, "Onyx");  // server name

                        // Prepare char list data
                        for (int i = 0; i < 16; ++i)
                        {
                            CharList.BlockCopy(ReserveDataA1.At(32), (32 + 140 * i), 140);
                            CharList.Fill(32 + 140 * i, 0x00, 4);
                            UList.Fill(16 * (i + 1), 0x00, 4);
                        }

                        UList.Set<byte>(0, 0x03);

                        int j = 0;

                        // TODO: get character list from database
                        List<Player> CharacterList = DBClient.GetMany<Player>(DBREQUESTTYPE.PLAYER, p => p.AccountId == client.Session.Account_id);
                        client.Session.Char_id_list = new List<uint>();
                        client.Session.Char_id_list.Clear();
                        if (CharacterList != null && CharacterList.Count > 0)
                            foreach (Player C in CharacterList)
                            {
                                if (ConfigHandler.MaintConfig.MaintMode == 0 || C.GMLevel > 0)
                                {
                                    // Add character id to session valid char id list
                                    client.Session.Char_id_list.Add(C.PlayerId);

                                    // Content Ids
                                    UList.Set<uint>(16 * (j + 1), C.PlayerId);
                                    CharList.Set<uint>(32 + 140 * j, C.PlayerId);
                                    UList.Set<uint>(20 * (j + 1), C.PlayerId);

                                    CharList.Set<uint>(4 + 32 + j * 140, C.PlayerId);

                                    CharList.Set<string>(12 + 32 + j * 140, C.Name);

                                    CharList.Set<byte>(46 + 32 + j * 140, C.Stats.Job);
                                    CharList.Set<byte>(73 + 32 + j * 140, C.Stats.JobLevel);

                                    CharList.Set<byte>(44 + 32 + j * 140, C.Look.Model.Race);
                                    CharList.Set<byte>(56 + 32 + j * 140, C.Look.Model.Face);
                                    CharList.Set<ushort>(58 + 32 + j * 140, C.Look.Head);
                                    CharList.Set<ushort>(60 + 32 + j * 140, C.Look.Body);
                                    CharList.Set<ushort>(62 + 32 + j * 140, C.Look.Hands);
                                    CharList.Set<ushort>(64 + 32 + j * 140, C.Look.Legs);
                                    CharList.Set<ushort>(66 + 32 + j * 140, C.Look.Feet);
                                    CharList.Set<ushort>(68 + 32 + j * 140, C.Look.Main);
                                    CharList.Set<ushort>(70 + 32 + j * 140, C.Look.Sub);

                                    CharList.Set<byte>(72 + 32 + j * 140, (byte)C.Location.CurrentZone);
                                    CharList.Set<ushort>(78 + 32 + j * 140, C.Location.CurrentZone);
                                    j++;
                                }
                            }

                        md5Hash = MD5.Create();                        
                        if (ConfigHandler.MaintConfig.MaintMode > 0 && j == 0)
                        {
                            SendLobbyError(client);
                        }
                        else
                        {
                            UList.Set<byte>(1, 0x10);
                            client.Session.DataSend(UList.Get(), 328);

                            hash = md5Hash.ComputeHash(CharList.Get(), 0, 2272);
                            CharList.BlockCopy(hash, 12, 16);
                            client.Session.ViewSend(CharList.Get(), 2272);
                        }
                    } else
                    {
                        SendLobbyError(client);
                    }

                    break;

                case 0xA2:
                    ByteRef ReserveDataA2 = new ByteRef(new byte[] {
                        0x48, 0x00, 0x00, 0x00, 0x49, 0x58, 0x46, 0x46, 0x0b, 0x00, 0x00, 0x00, 0x30, 0xD0, 0x10, 0xDC,
		                0x87, 0x64, 0x4B, 0x34, 0x72, 0x9A, 0x51, 0x23, 0x54, 0x14, 0x67, 0xF0, 0x82, 0xB2, 0xc0, 0x00,
		                0xC3, 0x57, 0x00, 0x00, 0x52, 0x65, 0x67, 0x69, 0x75, 0x7A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
		                0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x7F, 0x00, 0x00, 0x01, 0xd6, 0xd3, 0x00, 0x00,
		                0x7F, 0x00, 0x00, 0x01, 0xf2, 0xd2, 0x00, 0x00
                    });

                    ByteRef key3 = new ByteRef(20);
                    key3.BlockCopy(data.Skip(1).ToArray(), 0, key3.Length);
                    key3.Set<byte>(16, key3.GetByte(16) - 2);

                    ByteRef MainReserveData = new ByteRef(0x48);
                    md5Hash = MD5.Create();

                    uint charid = client.Session.Char_id;
                    if (client.Session.Char_id_list.Contains(charid))
                    {
                        // TODO: get character from database
                        ZoneChar zoneChar = new ZoneChar(); // MySQL.GetZoneChar(charid);
                        zoneChar.Account_id = 1001;
                        zoneChar.Zone_ip_str = "127.0.0.1";
                        zoneChar.Zone_ip = Utility.IPToInt("127.0.0.1", false);
                        zoneChar.Zone_port = 54240;
                        
                        if (zoneChar != null) {

                            if (zoneChar.Prev_zone == 0)
                                key3.Set<byte>(16, key3.GetByte(16) + 6);

                            if (ConfigHandler.MaintConfig.MaintMode == 0 || zoneChar.Gm_level > 0)
                            {
                                // TODO: handle session clearing 
                                //MySQL.ClearAccountSession(zoneChar.Account_id);

                                // TODO: update characters previous zone
                                //if (zoneChar.Prev_zone == 0)
                                //    MySQL.UpdateCharPrevZone(charid, zoneChar.Zone_id);

                                // TODO: validate session for character
                                bool accountValidated = true;
                                if (accountValidated) // !MySQL.ValidAccountSession(zoneChar.Account_id))
                                {
                                    string sessionKey = Utility.ByteArrayToString(key3.Get(), "");
                                    Console.WriteLine("SESSIONKEY: " + sessionKey);
                                    client.Session.Session_key = sessionKey;
                                    // TODO: create account session
                                    //MySQL.CreateAccountSession(zoneChar, charid, client.Session.Ip_address, sessionKey, client.Session.Version_mismatch);

                                    if (ConfigHandler.LoginConfig.LogUserIP == true)
                                    {
                                        // TODO: handle ip logging
                                        //MySQL.CreateIPRecord(zoneChar, charid, client.Session.Ip_address, client.Session.Mac_address);
                                    }

                                    ReserveDataA2.Set<uint>(0x38, zoneChar.Zone_ip);     // server IP integer
                                    ReserveDataA2.Set<ushort>(0x3C, zoneChar.Zone_port); // server port

                                    ReserveDataA2.Set<uint>(0x40, client.Session.Server_ip);  // search server IP integer                    
                                    ReserveDataA2.Set<uint>(0x44, ConfigHandler.LoginConfig.SearchServerPort);       // search server port

                                    MainReserveData.BlockCopy(ReserveDataA2, 0, 0x48);

                                    MD5 md5HashA2 = MD5.Create();
                                    MainReserveData.Fill(12, 0, 16);
                                    byte[] hasha2 = md5HashA2.ComputeHash(MainReserveData.Get(), 0, 0x48);
                                    MainReserveData.BlockCopy(hasha2, 12, 16);

                                    if (client.Session.View_client != null && client.Session.View_client.Connected)
                                    {
                                        client.Session.ViewSend(MainReserveData.Get(), 0x48);
                                        client.Session.View_client.Client.Disconnect(true);
                                    }

                                    client.Session.Status = SESSIONSTATUS.INGAME;
                                } else
                                {
                                    SendLobbyError(client);
                                }
                            } else
                            {
                                SendLobbyError(client);
                            }
                        }
                    } else
                    {
                        SendLobbyError(client);
                    }
                    break;
                default:
                    SendLobbyError(client);
                    break;
            }

            return 1;
        }

        private static int DataDisconnectHandler(SessionTcpClient client)
        {
            if (!client.IsDead)
            {
                var addr = ((IPEndPoint)client.Client.RemoteEndPoint).Address;
                var port = ((IPEndPoint)client.Client.RemoteEndPoint).Port;
                Logger.Info("Data Server Disconnection : {0} : {1}", new object[] { addr.ToString(), port });
            }
            SessionHandler.CheckSessionHealth(client.Session);
            return 1;
        }

        public static void Initialize(string address, int port)
        {
            dataServer = new TCPServer("Data Server", address, port, DataConnectHandler, DataDataHandler, DataDisconnectHandler);
        }
    }
}
