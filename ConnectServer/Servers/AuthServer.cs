using ConnectServer;
using Data.Game;
using DatabaseClient;
using Networking;
using System;
using System.Net;
using Toolbelt;
using static Ansi.AnsiFormatter;

namespace Servers
{
    public static class AuthServer
    {
        public static TCPServer authServer;

        private static int AuthConnectHandler(SessionTcpClient client)
        {
            var addr = ((IPEndPoint)client.Client.RemoteEndPoint).Address;
            var port = ((IPEndPoint)client.Client.RemoteEndPoint).Port;
            Logger.Info("Auth Server Connection : {0} : {1}", new object[] { addr.ToString(), port });

            LoginSession session = new LoginSession();
            session.Entry_time = DateTime.Now;
            session.Ip_address = addr.ToString();
            session.Status = SESSIONSTATUS.LOGGINGIN;
            client.Session = session;
            session.Auth_client = client;
            SessionHandler.AddSession(session);

            return 1;
        }

        private static bool IsIPBanned(string IP)
        {
            // TODO: handle ip ban check
            return false;
            //return MySQL.CheckIPBanned(IP);
        }

        private static int AuthDataHandler(SessionTcpClient client, byte[] data, int Length)
        {
            ByteRef response = new ByteRef(33);
            int MaxUserName = 16;
            int MaxPassword = 16;
            int MaxMac = 24;

            int Success = 1;

            byte Action = data[32];

            // Check if IP banned
            if (!IsIPBanned(client.Session.Ip_address))
            {
                byte[] bUsername = new byte[MaxUserName];
                byte[] bPassword = new byte[MaxPassword];
                byte[] bMac = new byte[MaxMac];

                Array.Copy(data, 0, bUsername, 0, MaxUserName);
                Array.Copy(data, MaxUserName, bPassword, 0, MaxPassword);
                Array.Copy(data, 0xC0, bMac, 0, MaxMac);

                string Username = Utility.ReadCString(bUsername);
                string Password = Utility.ReadCString(bPassword);
                string Mac = Utility.ReadCString(bMac);

                client.Session.Mac_address = Mac;

                // Check for valid string input
                if (!Utility.ValidAuthString(Username, MaxUserName) || !Utility.ValidAuthString(Password, MaxPassword))
                {
                    //bad characters in username or password
                    Logger.Warning("Invalid characters sent from: {0}", new object[]{ client.Session.Ip_address });
                    response.Resize(1);
                    response.Set<byte>(0, LOGINRESULT.ERROR);
                    Success = 0;
                }
                else
                {                                  
                    Username = Username.Trim();
                    Password = Password.Trim();

                    // TODO: check to see if account locked out

                    if (Action == (byte)LOGINRESULT.ATTEMPT)
                    {
                        Account acc = DBClient.GetOne<Account>(DBREQUESTTYPE.ACCOUNT, a => a.Username.Equals(Username));

                        if (acc != null && acc.Password.Equals(Utility.GenMD5(Password)))
                        {
                            uint AccountID = acc.AccountId;

                            // TODO: handle maintenance mode and gm accounts
                            if (ConfigHandler.MaintConfig.MaintMode > 0) // && !MySQL.IsGMAccount(AccountID))
                            {
                                response.Fill(0, 0x00, 33);
                                Success = 0;
                            }
                            else if (acc.Status.HasFlag(ACCOUNTSTATUS.NORMAL))
                            {
                                if (SessionHandler.AlreadyLoggedIn(AccountID))
                                {
                                    Success = 0;

                                    // TODO: might want to see about killing the other logged in session, but we'll see if it's necessary

                                }
                                else
                                {
                                    // TODO: reset accounts last modification date
                                    //MySQL.ResetAccountLastModify(AccountID);

                                    client.Session.Account_id = AccountID;
                                    client.Session.Session_hash = Utility.GenerateSessionHash(AccountID, Username);
                                    response.Set<byte>(0, LOGINRESULT.SUCCESS);
                                    response.Set<uint>(1, client.Session.Account_id);
                                    response.BlockCopy(client.Session.Session_hash, 5, 16);
                                    client.Session.Status = SESSIONSTATUS.ACCEPTINGTERMS;
                                    Success = 1;
                                }
                            }
                            else if (acc.Status.HasFlag(ACCOUNTSTATUS.BANNED))
                            {
                                response.Fill(0, 0x00, 33);
                                Success = 0;
                            }
                        }
                        else
                        {
                            //if (acc != null && acc.Locked)
                            //{
                                //  TODO: increment attempts in accounts table
                                //uint newLockTime = 0;
                                //if (attempts + 1 >= 20) newLockTime = 172800; // 48 hours
                                //else if (attempts + 1 == 10) newLockTime = 3600; // 1 hour
                                //else if (attempts + 1 == 5) newLockTime = 900; // 15 minutes
                                //fmtQuery = "UPDATE accounts SET attempts = %u, lock_time = UNIX_TIMESTAMP(NOW()) + %u WHERE id = %d;";
                                //if (Sql_Query(SqlHandle, fmtQuery, attempts + 1, newLockTime, accountId) == SQL_ERROR)
                                //    ShowError("Failed to update lock time for account: %s\n", name.c_str());
                            //}
                            Logger.Warning("Invalid login attempt for: {0}", new object[] { Username });
                            response.Resize(1);
                            response.Set<byte>(0, LOGINRESULT.ERROR);
                            Success = 0;
                        }
                    }
                    else if (Action == (byte)LOGINRESULT.CREATE)
                    {
                        response.Resize(1);
                        if (ConfigHandler.MaintConfig.MaintMode == 0)
                        {
                            Account acc = DBClient.GetOne<Account>(DBREQUESTTYPE.ACCOUNT, a => a.Username.Equals(Username));
                            bool accountexists = (acc != null);

                            uint maxAccountID = DBClient.GetMaxID(DBREQUESTTYPE.ACCOUNT);

                            if (!accountexists && maxAccountID > 0)
                            {
                                uint AccountID = maxAccountID + 1;

                                Account newAccount = new Account()
                                {
                                    AccountId = maxAccountID + 1,
                                    Username = Username,
                                    Password = Utility.GenMD5(Password),
                                    TimeCreate = Utility.Timestamp(),
                                    TimeLastModify = Utility.Timestamp(),
                                    // TODO: make these configurable
                                    ContentIds = 3,
                                    Expansions = 14,
                                    Features = 13,
                                    Status = ACCOUNTSTATUS.NORMAL
                                };

                                bool success = DBClient.InsertOne<Account>(DBREQUESTTYPE.ACCOUNT, newAccount);
                                                                
                                if (success) 
                                {
                                    response.Set<byte>(0, LOGINRESULT.ERROR_CREATE);
                                    Success = 0;
                                }
                                else
                                {
                                    response.Set<byte>(0, LOGINRESULT.SUCCESS_CREATE);
                                    Success = 1;
                                }
                            }
                            else
                            {
                                response.Set<byte>(0, LOGINRESULT.ERROR_CREATE);
                                Success = 0;
                            }
                        } else
                        {
                            response.Set<byte>(0, LOGINRESULT.ERROR);
                            Success = 0;
                        }
                    }
                }
            }
            else
            {
                response.Set<byte>(0, LOGINRESULT.ERROR_CREATE);
                Success = 0;
            }
            if (Success >= 0)
            {
                client.Session.AuthSend(response.Get());
                if (Success == 0)
                {
                    SessionHandler.KillSession(client.Session);
                }
            }
            return Success;
        }

        private static int AuthDisconnectHandler(SessionTcpClient client)
        {
            if (!client.IsDead)
            {
                var addr = ((IPEndPoint)client.Client.RemoteEndPoint).Address;
                var port = ((IPEndPoint)client.Client.RemoteEndPoint).Port;
                Logger.Info("Auth Server Disconnection : {0} : {1}", new object[] { addr.ToString(), port });
                client.Session.Auth_client.Dispose();
            }
            SessionHandler.CheckSessionHealth(client.Session);
            return 1;
        }

        public static void Initialize(string address, int port)
        {
            authServer = new TCPServer("Auth Server", address, port, AuthConnectHandler, AuthDataHandler, AuthDisconnectHandler);
        }

    }
}
