using Data.Game.Entities;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Game
{
    [Serializable]
    [BsonIgnoreExtraElements]
    public class Account : IQueryResult
    {
        public uint AccountId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        [BsonIgnoreIfNull]
        public string Email { get; set; }
        public uint TimeCreate { get; set; }
        public uint TimeLastModify { get; set; }
        public byte ContentIds { get; set; }
        public ushort Expansions { get; set; }
        public byte Features { get; set; }
        public uint LockoutTime { get; set; }
        public ushort LoginAttempts { get; set; }

        public ACCOUNTSTATUS Status { get; set; }

        public uint LockTime { get; set; }
        public uint LockDuration { get; set; }

        public uint BanTime { get; set; }
        public uint BanDuration { get; set; }

        public uint[] PlayerIds { get; set; }

        public Player[] Players;

        public bool ValidateLogin(string username, string password)
        {
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                if (username.Equals(Username) && password == Password)
                    return true;
            }
            return false;
        }
    }
}
