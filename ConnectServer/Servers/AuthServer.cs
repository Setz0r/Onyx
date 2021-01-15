using ConnectServer;
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

        private static int AuthDataHandler(SessionTcpClient client, Byte[] data, int Length)
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
                Byte[] bUsername = new Byte[MaxUserName];
                Byte[] bPassword = new Byte[MaxPassword];
                Byte[] bMac = new Byte[MaxMac];

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
                    // TODO: sanitize username and password
                    
                    // TODO: check to see if account locked out

                    if (Action == (byte)LOGINRESULT.ATTEMPT)
                    {
                        // TODO: get account from database
                        LoginAccount account = new LoginAccount();//MySQL.GetAccount(Username);

                        // TODO: verify login and return account id
                        uint AccountID = 1; // MySQL.VerifyLogin(Username, Password);
                        if (AccountID >= 1000)
                        {
                            // TODO: handle maintenance mode and gm accounts

                            if (account.Status.HasFlag(ACCOUNTSTATUS.NORMAL))
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

                                    // TODO: apparently DSP's login server sends a MSG_LOGIN to the server, but it's currently defunct in the gameservers msg handler

                                    client.Session.Account_id = AccountID;
                                    client.Session.Session_hash = Utility.GenerateSessionHash(AccountID, Username);
                                    response.Set<byte>(0, LOGINRESULT.SUCCESS);
                                    response.Set<uint>(1, client.Session.Account_id);
                                    response.BlockCopy(client.Session.Session_hash, 5, 16);
                                    client.Session.Status = SESSIONSTATUS.ACCEPTINGTERMS;
                                    Success = 1;
                                }
                            }
                            else if (account.Status.HasFlag(ACCOUNTSTATUS.BANNED))
                            {
                                response.Fill(0, 0x00, 33);
                                Success = 0;
                            }
                        }
                        else
                        {
                            if (account != null && account.Locked)
                            {
                                //  TODO: increment attempts in accounts table
                                //uint newLockTime = 0;
                                //if (attempts + 1 >= 20) newLockTime = 172800; // 48 hours
                                //else if (attempts + 1 == 10) newLockTime = 3600; // 1 hour
                                //else if (attempts + 1 == 5) newLockTime = 900; // 15 minutes
                                //fmtQuery = "UPDATE accounts SET attempts = %u, lock_time = UNIX_TIMESTAMP(NOW()) + %u WHERE id = %d;";
                                //if (Sql_Query(SqlHandle, fmtQuery, attempts + 1, newLockTime, accountId) == SQL_ERROR)
                                //    ShowError("Failed to update lock time for account: %s\n", name.c_str());
                            }
                            Logger.Warning("Invalid login attempt for: {0}", new object[] { Username });
                            response.Resize(1);
                            response.Set<byte>(0, LOGINRESULT.ERROR);
                            Success = 0;
                        }
                    }
                    else if (Action == (byte)LOGINRESULT.CREATE)
                    {
                        response.Resize(1);
                        // TODO: Check to see if this is needed.
                        //if (ConfigHandler.MaintConfig.MaintMode == 0)
                        //{
                        //    // TODO: check if account exists in database
                        //    bool accountexists = false;
                        //    if (accountexists) //!MySQL.AccountExists(Username))
                        //    {
                        //        // TODO: get next account id from database
                        //        uint AccountID = 1; // MySQL.GetNextAccountID();
                        //        
                        //        // TODO: create account in database
                        //        if (AccountID==1) //MySQL.CreateAccount(AccountID, Username, Password, ACCOUNTSTATUS.NORMAL, Privilege.USER) == -1)
                        //        {
                        //            response.Set<byte>(0, LOGINRESULT.ERROR_CREATE);
                        //            Success = 0;
                        //        }
                        //        else
                        //        {
                        //            response.Set<byte>(0, LOGINRESULT.SUCCESS_CREATE);
                        //            Success = 1;
                        //        }
                        //    }
                        //    else
                        //    {
                        //        response.Set<byte>(0, LOGINRESULT.ERROR_CREATE);
                        //        Success = 0;
                        //    }
                        //} else
                        response.Set<byte>(0, LOGINRESULT.ERROR);
                        Success = 0;
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
