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
        public int AccountId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public uint TimeCreate { get; set; }
        public uint TimeLastModify { get; set; }
        public byte ContentIds { get; set; }
        public uint LockoutTime { get; set; }
        public ushort LoginAttempts { get; set; }

        public UInt32[] PlayerIds { get; set; }

        public Player[] Players;

        public bool ValidateLogin(string username, string password)
        {
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {

                
            }
            return false;
        }
    }
}
