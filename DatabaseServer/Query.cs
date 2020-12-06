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

    }
}
