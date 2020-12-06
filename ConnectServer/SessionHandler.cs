using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Toolbelt;
using System;
using Networking;

namespace ConnectServer
{
    public static class SessionHandler
    {
        public static List<LoginSession> _sessions;

        public static void Initialize()
        {
            _sessions = new List<LoginSession>();
        }

        public static void AddSession(LoginSession session)
        {
            _sessions.Add(session);
            Logger.Info("Adding Session, Count: {0} : ThreadCount: {1}", new object[] { _sessions.Count, Process.GetCurrentProcess().Threads.Count });
            Logger.Info("Total Memory: {0}", new object[] { GC.GetTotalMemory(false) });
        }

        public static bool CharNameExists(string charName)
        {
            foreach (var s in _sessions)
            {
                if (s.Char_name == charName)
                    return true;
            }

            return false;
        }

        public static bool AlreadyLoggedIn(uint accountID)
        {
            //@todo: check if player already logged in
            List<LoginSession> sessions = new List<LoginSession>(); //MySQL.GetAccountsSessions();

            bool SessionInDB = sessions.Exists(x => x.Account_id == accountID);
            bool SessionInCS = _sessions.Exists(x => x.Account_id == accountID);
            
            if (SessionInCS && SessionInDB)
                return true;
            if (SessionInCS && !SessionInDB)
            {
                LoginSession s = _sessions.Find(x => x.Account_id.Equals(accountID));
                if (s != null)
                    KillSession(s);
            }

            return false;
        }

        public static LoginSession GetActiveSession(uint accountID)
        {
            foreach (var s in _sessions)
            {
                if (s.Account_id == accountID)
                    return s;
            }

            return null;
        }

        public static void KillSession(LoginSession session)
        {
            if (_sessions.Contains(session))
            {
                if (session.Auth_client != null)
                {
                    if (session.Auth_client.Connected)
                        session.Auth_client.Close();
                    else
                        session.Auth_client.Dispose();
                }

                if (session.View_client != null)
                {
                    if (session.View_client.Connected)
                        session.View_client.Close();
                    else
                        session.View_client.Dispose();
                }

                if (session.Data_client != null)
                {
                    if (session.Data_client.Connected)
                        session.Data_client.Close();
                    else
                        session.Data_client.Dispose();
                }

                _sessions.Remove(session);
                Logger.Info("Removing Session, Count: {0} : ThreadCount: {1}", new object[] { _sessions.Count, Process.GetCurrentProcess().Threads.Count });
                Logger.Info("Total Memory: {0}", new object[] { GC.GetTotalMemory(false) });
            }
        }

        public static LoginSession GetSessionByHash(string ip, byte[] hash)
        {
            foreach (LoginSession session in _sessions)
            {
                if (session.Ip_address == ip && 
                    session.Session_hash.SequenceEqual(hash))
                {
                    return session;
                }
            }
            return null;
        }

        public static LoginSession GetSessionByIP(string ip, SESSIONSTATUS status)
        {
            foreach (LoginSession session in _sessions)
            {
                if (session.Ip_address == ip && session.Status == status)
                {
                    return session;
                }
            }
            return null;
        }

        public static LoginSession GetSessionByAccountID(string ip, SESSIONSTATUS status, uint id)
        {
            foreach (LoginSession session in _sessions)
            {
                if (session.Ip_address == ip &&
                    session.Status == status &&
                    session.Account_id == id)
                {
                    return session;
                }
            }
            return null;
        }

        public static bool VerifySockets(LoginSession session, bool checkAuth, bool checkView, bool checkData)
        {
            bool Sockets_ok = true;

            if (checkAuth && !session.Auth_client.Connected)
                return false;

            if (checkView && !session.View_client.Connected)
                return false;

            if (checkData && !session.Data_client.Connected)
                return false;

            return Sockets_ok;
        }

        private static bool AuthDead(LoginSession session)
        {
            return (session.Auth_client == null || !session.Auth_client.Connected);
        }

        private static bool ViewDead(LoginSession session)
        {
            return (session.View_client == null || !session.View_client.Connected);
        }

        private static bool DataDead(LoginSession session)
        {
            return (session.Data_client == null || !session.Data_client.Connected);
        }

        public static void CheckSessionHealth(LoginSession session)
        {
            if (session == null) return;
            if ((session.Status < SESSIONSTATUS.ACCEPTINGTERMS && AuthDead(session) && ViewDead(session) && DataDead(session)) ||
               ((session.Status > SESSIONSTATUS.ACCEPTINGTERMS && session.Status < SESSIONSTATUS.INGAME) && ViewDead(session) && DataDead(session)) ||
               (session.Status == SESSIONSTATUS.INGAME && DataDead(session)))
            {
                KillSession(session);
            }
        }

        public static void Cleanup()
        {
            foreach (LoginSession session in _sessions)
            {
                switch (session.Status)
                {
                    case SESSIONSTATUS s when s > SESSIONSTATUS.NONE && s < SESSIONSTATUS.INGAME:

                        break;
                    case SESSIONSTATUS.NONE:
                    case SESSIONSTATUS.LOGGINGIN:
                    case SESSIONSTATUS.ACCEPTINGTERMS:
                    case SESSIONSTATUS.SYNCHRONIZING:
                    case SESSIONSTATUS.MAINMENU:
                    case SESSIONSTATUS.CHARSELECT:
                    case SESSIONSTATUS.CHARCREATE:
                    case SESSIONSTATUS.CHARDELETE:
                    case SESSIONSTATUS.INGAME:
                        break;
                    case SESSIONSTATUS.DISCONNECTING:
                        break;
                    case SESSIONSTATUS.DISCONNECTED:
                        break;
                }
                if (session.Status == SESSIONSTATUS.DISCONNECTING)
                {

                }
            }
        }
    }
}
