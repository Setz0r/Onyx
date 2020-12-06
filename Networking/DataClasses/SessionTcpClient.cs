using System.IO;
using System.Net.Sockets;

namespace Networking
{
    public class SessionTcpClient : TcpClient
    {
        public StreamReader Reader { get; }
        public StreamWriter Writer { get; }
        private LoginSession _session;
        public bool IsDead { get; set; }
        public SessionTcpClient(Socket acceptedSocket)
        {
            this.Client.Dispose();
            this.Client = acceptedSocket;
            this.Active = true;
            Reader = new StreamReader(this.GetStream());
            Writer = new StreamWriter(this.GetStream());
            _session = null;
        }
        public LoginSession Session { get => _session; set => _session = value; }
        protected override void Dispose(bool disposing)
        {
            IsDead = true;
            base.Dispose(disposing);
        }
    }
}
