using System;

namespace Networking
{
    public struct AccountLockInfo
    {
        public uint AccountID;
        public ushort Attempts;
        public uint LockTime;
        public bool Locked;
    }

    public struct AccountEF
    {
        public ushort Expansions;
        public ushort Features;
    }

    public class LoginAccount
    {
        private uint _ID;
        private string _Login;
        private string _Password;
        private string _Email;
        private string _Email2;
        private DateTime _TimeCreate;
        private DateTime _TimeLastModify;
        private int _ContentIDS;
        private int _Expansions;
        private int _Features;
        private int _Priv;
        private int _LockTime;
        private int _Attempts;
        private bool _Locked;

        public uint ID { get => _ID; set => _ID = value; }
        public string Login { get => _Login; set => _Login = value; }
        public string Password { get => _Password; set => _Password = value; }
        public string Email { get => _Email; set => _Email = value; }
        public string Email2 { get => _Email2; set => _Email2 = value; }
        public DateTime TimeCreate { get => _TimeCreate; set => _TimeCreate = value; }
        public DateTime TimeLastModify { get => _TimeLastModify; set => _TimeLastModify = value; }
        public int ContentIDS { get => _ContentIDS; set => _ContentIDS = value; }
        public int Expansions { get => _Expansions; set => _Expansions = value; }
        public int Features { get => _Features; set => _Features = value; }
        public int Priv { get => _Priv; set => _Priv = value; }
        public int LockTime { get => _LockTime; set => _LockTime = value; }
        public int Attempts { get => _Attempts; set => _Attempts = value; }
        public bool Locked { get => _Locked; set => _Locked = value; }
    }
}
