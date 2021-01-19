using Data;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using Toolbelt;

namespace Networking
{
    public class LoginSession
    {
        private SessionTcpClient _auth_client;
        private SessionTcpClient _data_client;
        private SessionTcpClient _view_client;
        private SessionTcpClient _lobby_client;

        private string _ip_address;
        private string _mac_address;
        private uint _server_ip;
        private uint _account_id;
        private string _session_key;
        private byte[] _session_hash;
        private uint _char_id;
        private string _char_name;
        private List<uint> _char_id_list;
        private byte _version_mismatch;

        private SESSIONSTATUS _status;

        private byte[] _auth_buffer;
        private byte[] _data_buffer;
        private byte[] _view_buffer;
        private byte[] _lobby_buffer;

        private DateTime _entry_time;
        private DateTime _connect_time;
        private DateTime _data_time;
        private DateTime _disconnect_time;
        
        public LoginSession()
        {
            _status = SESSIONSTATUS.NONE;
            _auth_buffer = new byte[2048];
            _data_buffer = new byte[2048];
            _view_buffer = new byte[2048];
            _lobby_buffer = new byte[2048];
        }

        public SessionTcpClient Auth_client { get => _auth_client; set => _auth_client = value; }

        public void AuthProcessData(byte[] data)
        {
            Logger.Info("AUTHPROCESSDATA: " + data.Length);
            
            Byte[] bUsername = new Byte[16];
            Byte[] bPassword = new Byte[16];

            Array.Copy(data, 0, bUsername, 0, 16);
            Array.Copy(data, 16, bPassword, 0, 16);

            String Username = Encoding.ASCII.GetString(bUsername).TrimEnd((Char)0);
            String Password = Encoding.ASCII.GetString(bPassword).TrimEnd((Char)0);

            ByteRef response = new ByteRef(33);

            response.Set<byte>(0, 0x01);
            response.Set<uint>(1, 21828);

            AuthSend(response.Get());
        }

        public void AuthStartReceive()
        {
            try
            {
                Auth_client.Client.BeginReceive(_auth_buffer, 0, _auth_buffer.Length, SocketFlags.None, AuthOnReceived, Auth_client.Client);
            }
            catch (SocketException e)
            {
                Console.WriteLine("AuthReceive Exception: " + e.Message);
            }
        }

        public void AuthOnReceived(IAsyncResult iAsyncResult)
        {
            try
            {
                var bytesReceived = Auth_client.Client.EndReceive(iAsyncResult);

                if (bytesReceived == 0)
                {
                    return;
                }

                var packet = new byte[bytesReceived];
                Array.Copy(_auth_buffer, packet, bytesReceived);
                AuthProcessData(packet);
            }
            catch (Exception e)
            {
                Console.WriteLine("AuthOnReceived Exception: " + e.Message);
            }
            finally
            {
                try
                {
                    Auth_client.Client.BeginReceive(_auth_buffer, 0, _auth_buffer.Length, SocketFlags.None, AuthOnReceived, Auth_client.Client);
                }
                catch (Exception e)
                {
                    Console.WriteLine("AuthReceive Exception: " + e.Message);
                }
            }
        }

        public void AuthSend(byte[] data)
        {
            if (_auth_client != null && !_auth_client.IsDead && _auth_client.Connected)
                _auth_client.Client.BeginSend(data, 0, data.Length, 0, AuthOnSend, null);
        }

        public void AuthOnSend(IAsyncResult asyncResult)
        {
            try
            {
                if (Auth_client.Client == null)
                {
                    return;
                }

                Auth_client.Client.EndSend(asyncResult);
            }
            catch (Exception e)
            {
                Console.WriteLine("AuthOnSend Exception: " + e.Message);
            }
        }

        public SessionTcpClient Data_client { get => _data_client; set => _data_client = value; }

        public void DataReceive(byte[] data, int length)
        {
            try
            {
                _data_client.Client.BeginReceive(_data_buffer, 0, _data_buffer.Length, SocketFlags.None, DataOnReceived, _data_client.Client);
            }
            catch (SocketException e)
            {
                Console.WriteLine("DataReceive Exception: " + e.Message);
            }
        }
        public void DataOnReceived(IAsyncResult iAsyncResult)
        {
            try
            {
                var bytesReceived = _data_client.Client.EndReceive(iAsyncResult);

                if (bytesReceived == 0)
                {
                    return;
                }

                var packet = new byte[bytesReceived];
                Array.Copy(_data_buffer, packet, bytesReceived);
            }
            catch (Exception e)
            {
                Console.WriteLine("DataOnReceived Exception: " + e.Message);
            }
            finally
            {
                try
                {
                    _data_client.Client.BeginReceive(_data_buffer, 0, _data_buffer.Length, SocketFlags.None, DataOnReceived, _data_client.Client);
                }
                catch (Exception e)
                {
                    Console.WriteLine("DataReceive Exception: " + e.Message);
                }
            }
        }

        public void DataSend(byte[] data)
        {
            if (_data_client != null && !_data_client.IsDead && _data_client.Connected)
                _data_client.Client.BeginSend(data, 0, data.Length, 0, DataOnSend, null);
        }

        public void DataSend(byte[] data, int length)
        {
            if (_data_client != null && !_data_client.IsDead && _data_client.Connected)
                _data_client.Client.BeginSend(data, 0, length, 0, DataOnSend, null);
        }

        public void DataOnSend(IAsyncResult asyncResult)
        {
            try
            {
                if (_data_client.Client == null)
                {
                    return;
                }

                _data_client.Client.EndSend(asyncResult);
            }
            catch (Exception e)
            {
                Console.WriteLine("DataOnSend Exception: " + e.Message);
            }
        }

        public SessionTcpClient View_client { get => _view_client; set => _view_client = value; }

        public void ViewReceive(byte[] data, int length)
        {
            try
            {
                _view_client.Client.BeginReceive(_view_buffer, 0, _view_buffer.Length, SocketFlags.None, ViewOnReceived, _view_client.Client);
            }
            catch (SocketException e)
            {
                Console.WriteLine("ViewReceive Exception: " + e.Message);
            }
        }

        public void ViewOnReceived(IAsyncResult iAsyncResult)
        {
            try
            {
                var bytesReceived = _view_client.Client.EndReceive(iAsyncResult);

                if (bytesReceived == 0)
                {
                    return;
                }

                var packet = new byte[bytesReceived];
                Array.Copy(_view_buffer, packet, bytesReceived);                
            }
            catch (Exception e)
            {
                Console.WriteLine("ViewOnReceived Exception: " + e.Message);
            }
            finally
            {
                try
                {
                    _view_client.Client.BeginReceive(_view_buffer, 0, _view_buffer.Length, SocketFlags.None, ViewOnReceived, _view_client.Client);
                }
                catch (Exception e)
                {
                    Console.WriteLine("ViewReceive Exception: " + e.Message);
                }
            }
        }

        public void ViewSend(byte[] data)
        {
            if (_view_client != null && !_view_client.IsDead && _view_client.Connected)
                _view_client.Client.BeginSend(data, 0, data.Length, 0, ViewOnSend, null);
        }

        public void ViewSend(byte[] data, int length)
        {
            if (_view_client != null && !_view_client.IsDead && _view_client.Connected)
                _view_client.Client.BeginSend(data, 0, length, 0, ViewOnSend, null);
        }

        public void ViewOnSend(IAsyncResult asyncResult)
        {
            try
            {
                if (_view_client.Client == null)
                {
                    return;
                }

                _view_client.Client.EndSend(asyncResult);
            }
            catch (Exception e)
            {
                Console.WriteLine("ViewOnSend Exception: " + e.Message);
            }
        }

        public SessionTcpClient Lobby_client { get => _lobby_client; set => _lobby_client = value; }

        public void LobbyReceive(byte[] data, int length)
        {
            try
            {
                _lobby_client.Client.BeginReceive(_lobby_buffer, 0, _lobby_buffer.Length, SocketFlags.None, LobbyOnReceived, _lobby_client.Client);
            }
            catch (SocketException e)
            {
                Console.WriteLine("LobbyReceive Exception: " + e.Message);
            }
        }

        public void LobbyOnReceived(IAsyncResult iAsyncResult)
        {
            try
            {
                var bytesReceived = _lobby_client.Client.EndReceive(iAsyncResult);

                if (bytesReceived == 0)
                {
                    return;
                }

                var packet = new byte[bytesReceived];
                Array.Copy(_lobby_buffer, packet, bytesReceived);
            }
            catch (Exception e)
            {
                Console.WriteLine("LobbyOnReceived Exception: " + e.Message);
            }
            finally
            {
                try
                {
                    _lobby_client.Client.BeginReceive(_lobby_buffer, 0, _lobby_buffer.Length, SocketFlags.None, LobbyOnReceived, _lobby_client.Client);
                }
                catch (Exception e)
                {
                    Console.WriteLine("LobbyReceive Exception: " + e.Message);
                }
            }
        }

        public void LobbySend(byte[] data)
        {
            if (_lobby_client != null && !_lobby_client.IsDead && _lobby_client.Connected)
                _lobby_client.Client.BeginSend(data, 0, data.Length, 0, LobbyOnSend, null);
        }

        public void LobbySend(byte[] data, int length)
        {
            if (_lobby_client != null && !_lobby_client.IsDead && _lobby_client.Connected)
                _lobby_client.Client.BeginSend(data, 0, length, 0, LobbyOnSend, null);
        }

        public void LobbyOnSend(IAsyncResult asyncResult)
        {
            try
            {
                if (_lobby_client.Client == null)
                {
                    return;
                }

                _lobby_client.Client.EndSend(asyncResult);
            }
            catch (Exception e)
            {
                Console.WriteLine("LobbyOnSend Exception: " + e.Message);
            }
        }
         
        public string Ip_address { get => _ip_address; set => _ip_address = value; }
        public string Mac_address { get => _mac_address; set => _mac_address = value; }
        public uint Server_ip { get => _server_ip; set => _server_ip = value; }
        public uint Account_id { get => _account_id; set => _account_id = value; }

        public SESSIONSTATUS Status { get => _status; set => _status = value; }
        public DateTime Entry_time { get => _entry_time; set => _entry_time = value; }
        public DateTime Connect_time { get => _connect_time; set => _connect_time = value; }
        public DateTime Data_time { get => _data_time; set => _data_time = value; }
        public DateTime Disconnect_time { get => _disconnect_time; set => _disconnect_time = value; }

        public string Session_key { get => _session_key; set => _session_key = value; }
        public byte[] Session_hash { get => _session_hash; set => _session_hash = value; }
        public uint Char_id { get => _char_id; set => _char_id = value; }
        public string Char_name { get => _char_name; set => _char_name = value; }
        public List<uint> Char_id_list { get => _char_id_list; set => _char_id_list = value; }
        public byte Version_mismatch { get => _version_mismatch; set => _version_mismatch = value; }
    }
}
