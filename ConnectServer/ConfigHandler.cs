using System;
using System.Collections.Generic;
using Toolbelt;

namespace ConnectServer
{
    public struct VersionConfiguration
    {
        public string ClientVersion;
        public int VersionLock;
    }
    public struct MysqlConfiguration
    {
        public string Host;
        public int Port;
        public string Login;
        public string Password;
        public string Database;
    }
    public struct DatabaseConfiguration
    {
        public MysqlConfiguration Lobby;
        public MysqlConfiguration Search;
        public MysqlConfiguration Map;
    }
    public struct MaintenanceConfiguration
    {
        public int MaintMode;
    }
    public struct LoginConfiguration
    {
        public string TimestampFormat;
        public string StdoutWithAnsiSequence;
        public int ConsoleSilent;
        public string LoginDataIP;
        public int LoginDataPort;
        public string LoginViewIP;
        public int LoginViewPort;
        public string LoginAuthIP;
        public int LoginAuthPort;
        public int SearchServerPort;
        public string ServerName;
        public string MsgServerIP;
        public int MsgServerPort;
        public bool LogUserIP;
    }
    public static class ConfigHandler
    {
        public static Dictionary<string, string> versionConfig;
        public static VersionConfiguration VersionConfig;
        public static Dictionary<string, string> loginConfig;
        public static LoginConfiguration LoginConfig;
        public static Dictionary<string, string> maintConfig;
        public static MaintenanceConfiguration MaintConfig;
        public static Dictionary<string, string> databaseConfig;
        public static DatabaseConfiguration DatabaseConfig;
        public static void ParseVersionConfig()
        {
            if (versionConfig.ContainsKey("CLIENT_VER"))
            {
                VersionConfig.ClientVersion = versionConfig["CLIENT_VER"];
            }
            if (versionConfig.ContainsKey("VER_LOCK"))
            {
                VersionConfig.VersionLock = Convert.ToInt32(versionConfig["VER_LOCK"]);
            }
        }
        public static void ParseMaintConfig()
        {
            if (maintConfig.ContainsKey("MAINT_MODE"))
            {
                MaintConfig.MaintMode = Convert.ToInt32(maintConfig["MAINT_MODE"]);
            }
        }
        public static void ParseDatabaseConfig()
        {
            // Lobby
            if (databaseConfig.ContainsKey("mysql_lobby_host"))
            {
                DatabaseConfig.Lobby.Host = databaseConfig["mysql_lobby_host"];
            }
            if (databaseConfig.ContainsKey("mysql_lobby_port"))
            {
                DatabaseConfig.Lobby.Port = Convert.ToInt32(databaseConfig["mysql_lobby_port"]);
            }
            if (databaseConfig.ContainsKey("mysql_lobby_login"))
            {
                DatabaseConfig.Lobby.Login = databaseConfig["mysql_lobby_login"];
            }
            if (databaseConfig.ContainsKey("mysql_lobby_password"))
            {
                DatabaseConfig.Lobby.Password = databaseConfig["mysql_lobby_password"];
            }
            if (databaseConfig.ContainsKey("mysql_lobby_database"))
            {
                DatabaseConfig.Lobby.Database = databaseConfig["mysql_lobby_database"];
            }
            // Search
            if (databaseConfig.ContainsKey("mysql_search_host"))
            {
                DatabaseConfig.Search.Host = databaseConfig["mysql_search_host"];
            }
            if (databaseConfig.ContainsKey("mysql_search_port"))
            {
                DatabaseConfig.Search.Port = Convert.ToInt32(databaseConfig["mysql_search_port"]);
            }
            if (databaseConfig.ContainsKey("mysql_search_login"))
            {
                DatabaseConfig.Search.Login = databaseConfig["mysql_search_login"];
            }
            if (databaseConfig.ContainsKey("mysql_search_password"))
            {
                DatabaseConfig.Search.Password = databaseConfig["mysql_search_password"];
            }
            if (databaseConfig.ContainsKey("mysql_search_database"))
            {
                DatabaseConfig.Search.Database = databaseConfig["mysql_search_database"];
            }
            // Map
            if (databaseConfig.ContainsKey("mysql_map_host"))
            {
                DatabaseConfig.Map.Host = databaseConfig["mysql_map_host"];
            }
            if (databaseConfig.ContainsKey("mysql_map_port"))
            {
                DatabaseConfig.Map.Port = Convert.ToInt32(databaseConfig["mysql_map_port"]);
            }
            if (databaseConfig.ContainsKey("mysql_map_login"))
            {
                DatabaseConfig.Map.Login = databaseConfig["mysql_map_login"];
            }
            if (databaseConfig.ContainsKey("mysql_map_password"))
            {
                DatabaseConfig.Map.Password = databaseConfig["mysql_map_password"];
            }
            if (databaseConfig.ContainsKey("mysql_map_database"))
            {
                DatabaseConfig.Map.Database = databaseConfig["mysql_map_database"];
            }
        }
        public static void ParseLoginConfig()
        {
            if (loginConfig.ContainsKey("timestamp_format"))
            {
                LoginConfig.TimestampFormat = loginConfig["timestamp_format"];
            }
            if (loginConfig.ContainsKey("stdout_with_ansisequence"))
            {
                LoginConfig.StdoutWithAnsiSequence = loginConfig["stdout_with_ansisequence"];
            }
            if (loginConfig.ContainsKey("console_silent"))
            {
                LoginConfig.ConsoleSilent = Convert.ToInt32(loginConfig["console_silent"]);
            }
            if (loginConfig.ContainsKey("login_data_ip"))
            {
                LoginConfig.LoginDataIP = loginConfig["login_data_ip"];
            }
            if (loginConfig.ContainsKey("login_data_port"))
            {
                LoginConfig.LoginDataPort = Convert.ToInt32(loginConfig["login_data_port"]);
            }
            if (loginConfig.ContainsKey("login_view_ip"))
            {
                LoginConfig.LoginViewIP = loginConfig["login_view_ip"];
            }
            if (loginConfig.ContainsKey("login_view_port"))
            {
                LoginConfig.LoginViewPort = Convert.ToInt32(loginConfig["login_view_port"]);
            }
            if (loginConfig.ContainsKey("login_auth_ip"))
            {
                LoginConfig.LoginAuthIP = loginConfig["login_auth_ip"];
            }
            if (loginConfig.ContainsKey("login_auth_port"))
            {
                LoginConfig.LoginAuthPort = Convert.ToInt32(loginConfig["login_auth_port"]);
            }
            if (loginConfig.ContainsKey("search_server_port"))
            {
                LoginConfig.SearchServerPort = Convert.ToInt32(loginConfig["search_server_port"]);
            }
            if (loginConfig.ContainsKey("servername"))
            {
                LoginConfig.ServerName = loginConfig["servername"];
            }
            if (loginConfig.ContainsKey("msg_server_ip"))
            {
                LoginConfig.MsgServerIP = loginConfig["msg_server_ip"];
            }
            if (loginConfig.ContainsKey("msg_server_port"))
            {
                LoginConfig.MsgServerPort = Convert.ToInt32(loginConfig["msg_server_port"]);
            }
            if (loginConfig.ContainsKey("log_user_ip"))
            {
                LoginConfig.LogUserIP = Convert.ToBoolean(loginConfig["log_user_ip"]);
            }
        }
        public static void ReadConfigs()
        {
            versionConfig = Utility.ReadConf(@"version.info");
            ParseVersionConfig();
            maintConfig = Utility.ReadConf(@"conf/maint.conf");
            ParseMaintConfig();
            databaseConfig = Utility.ReadConf(@"conf/mysql_config.conf");
            ParseDatabaseConfig();
            loginConfig = Utility.ReadConf(@"conf/login_darkstar.conf");
            ParseLoginConfig();
        }
    }
}
