namespace Data.Structs
{
    public class ZoneChar
    {
        private uint _zone_ip;
        private string _zone_ip_str;
        private ushort _zone_port;
        private ushort _zone_id;
        private ushort _prev_zone;
        private ushort _gm_level;
        private uint _account_id;

        public uint Zone_ip { get => _zone_ip; set => _zone_ip = value; }
        public string Zone_ip_str { get => _zone_ip_str; set => _zone_ip_str = value; }
        public ushort Zone_port { get => _zone_port; set => _zone_port = value; }
        public ushort Zone_id { get => _zone_id; set => _zone_id = value; }
        public ushort Prev_zone { get => _prev_zone; set => _prev_zone = value; }
        public ushort Gm_level { get => _gm_level; set => _gm_level = value; }
        public uint Account_id { get => _account_id; set => _account_id = value; }
    }
}
