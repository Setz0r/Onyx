using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Game
{
    [Serializable]
    [BsonIgnoreExtraElements]
    public class ActiveSession : IQueryResult
    {
        public uint AccountID { get; set; }
        public uint PlayerID { get; set; }
        public string EndPoint { get; set; }
        public string SessionHash { get; set; }
        public byte Zoning { get; set; }
        public ushort CurrentZoneID { get; set; }
        public ushort NextZoneID { get; set; }        
    }
}
