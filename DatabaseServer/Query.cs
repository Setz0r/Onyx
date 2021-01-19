using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver.Linq;
using MongoDB.Driver;
using System.Linq.Expressions;
using System.Linq;
using System.Linq.Dynamic.Core;
using Data.Game;
using System.IO;
using System.Runtime.Serialization;
using Toolbelt;
using System.Runtime.Serialization.Formatters.Binary;
using Data.Game.Entities;
using MongoDB.Bson.Serialization;

namespace DatabaseServer
{
    public static class Query
    {
        private static T Deserialize<T>(byte[] param)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream(param))
                {
                    IFormatter br = new BinaryFormatter();
                    return (T)br.Deserialize(ms);
                }
            }
            catch (Exception e)
            {
                Logger.Error("Unable to deserialize data sent from Database Client : {0}", new object[] { e.Message });
                return default(T);
            }
        }

        public static T QueryOne<T>(IMongoDatabase db, string collection, Expression exp) where T : IQueryResult, new()
        {
            var col = db.GetCollection<T>(collection);
            var query = col.AsQueryable<T>().Where((LambdaExpression)exp).FirstOrDefault();

            return query;
        }

        public static List<T> QueryMany<T>(IMongoDatabase db, string collection, Expression exp, ref uint resultCount) where T : IQueryResult, new()
        {
            var col = db.GetCollection<T>(collection);
            var query = col.AsQueryable<T>().Where((LambdaExpression)exp).ToDynamicList().Cast<T>().ToList();
            resultCount = (uint)query.Count();

            return query;
        }

        public static void InsertOne<T>(IMongoDatabase db, string collection, byte[] objData)
        {
            T obj = Deserialize<T>(objData);
            var col = db.GetCollection<T>(collection);
            col.InsertOne(obj);
        }

        public static void InsertMany<T>(IMongoDatabase db, string collection, byte[] objData)
        {
            List<T> objs = Deserialize<List<T>>(objData);
            var col = db.GetCollection<T>(collection);
            col.InsertMany(objs);
        }

        public static long UpdateOne<T>(IMongoDatabase db, string collection, Expression filterExp, Dictionary<string, object> update) where T : IQueryResult, new()
        {
            var col = db.GetCollection<T>(collection);
            BsonDocument updateItem = new BsonDocument("$set", new BsonDocument(update));
            var u = Builders<T>.Update.Combine(updateItem);
            var result = col.UpdateOne((FilterDefinition<T>)filterExp, u, new UpdateOptions { IsUpsert = false });
            return result.ModifiedCount;
        }

        public static long UpdateMany<T>(IMongoDatabase db, string collection, Expression filterExp, Dictionary<string, object> update) where T : IQueryResult, new()
        {
            var col = db.GetCollection<T>(collection);
            BsonDocument updateItem = new BsonDocument("$set", new BsonDocument(update));
            var u = Builders<T>.Update.Combine(updateItem);
            var result = col.UpdateMany((FilterDefinition<T>)filterExp, u, new UpdateOptions { IsUpsert = false });
            return result.ModifiedCount;
        }

        public static long DeleteOne<T>(IMongoDatabase db, string collection, Expression filterExp)
        {
            var col = db.GetCollection<T>(collection);
            var result = col.DeleteOne((FilterDefinition<T>)filterExp);
            return result.DeletedCount;
        }

        public static long DeleteMany<T>(IMongoDatabase db, string collection, Expression filterExp)
        {
            var col = db.GetCollection<T>(collection);
            var result = col.DeleteMany((FilterDefinition<T>)filterExp);
            return result.DeletedCount;
        }

        public static uint GetMaxAccountID(IMongoDatabase db, string collection)
        {
            uint accId = 1000;
            var col = db.GetCollection<Account>(collection);
            Account acc = col.AsQueryable().OrderByDescending(a => a.AccountId).FirstOrDefault();

            if (acc != null && acc.AccountId > 0)
                accId = (uint)acc.AccountId;

            return accId;
        }

        public static uint GetMaxPlayerID(IMongoDatabase db, string collection)
        {
            uint playerId = 1000;
            var col = db.GetCollection<Player>(collection);
            Player p = col.AsQueryable().OrderByDescending(p => p.PlayerId).FirstOrDefault();

            if (p != null && p.PlayerId > 0)
                playerId = (uint)p.PlayerId;

            return playerId;
        }
    }
}
