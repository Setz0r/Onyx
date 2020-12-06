using System;
using Data.FBEntities;
using Networking;
using MongoDB.Bson;
using MongoDB.Driver.Linq;
using MongoDB.Driver;
using System.Threading;
using Toolbelt;
using Data.FBObjects;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections.Generic;
using DatabaseClient;
using System.Reflection;
using Data.Game;
using static Toolbelt.Logger;
using Serialize.Linq.Serializers;
using System.Linq;
using Serialize.Linq.Nodes;
using Serialize.Linq.Extensions;

namespace DatabaseServer
{
    class Program
    {
        public static MongoClient dbClient;
        public static IMongoDatabase db;
        public static bool running = true;

        public static byte[] Serialize<T>(T obj)
        {
            byte[] response = null;
            try
            {
                var binaryFormatter = new BinaryFormatter();
                var memoryStream = new MemoryStream();
                binaryFormatter.Serialize(memoryStream, obj);
                response = memoryStream.ToArray();
            }
            catch (Exception e)
            {
                Logger.Error("Unable to serialize object of type : {0} : {1}", new object[] { typeof(T).ToString(), e.Message });
            }
            return response;
        }

        public static byte[] ObjectToByteArray(Object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        public static (Type, string) GetType(DBREQUESTTYPE requestType)
        {
            switch (requestType)
            {
                case DBREQUESTTYPE.ACCOUNT:
                    return (typeof(Account), "accounts");

            }

            return (null, "");
        }

        public static byte[] RequestHandler(byte[] data, int length)
        {
            byte[] response = new byte[] { 0 };
            DBREQUESTTYPE requestType;
            DBRESULTTYPE resultType;

            Console.WriteLine("Request handler fired : " + length.ToString());

            if (length > 2)
            {
                requestType = (DBREQUESTTYPE)data[0];
                resultType = (DBRESULTTYPE)data[1];

                (var type, string colname) = GetType(requestType);
                MethodInfo serialize = typeof(Program).GetMethod("Serialize", BindingFlags.Public | BindingFlags.Static).MakeGenericMethod(type);

                byte[] queryResponse = null;
                uint resultCount = 0;
                if (resultType <= DBRESULTTYPE.GETMANY)
                {
                    var serializer = new ExpressionSerializer(new BinarySerializer());
                    var exp = serializer.DeserializeBinary(data.Skip(2).ToArray());

                    ExpressionNode node = exp.ToExpressionNode();

                    if (resultType == DBRESULTTYPE.GETONE)
                    {
                        MethodInfo queryOne = typeof(Query).GetMethod("QueryOne", BindingFlags.Public | BindingFlags.Static).MakeGenericMethod(type);
                        var result = queryOne.Invoke(null, new object[] { db, colname, exp });
                        if (result != null)
                            resultCount = 1;
                        queryResponse = ObjectToByteArray(result);
                    }
                    else if (resultType == DBRESULTTYPE.GETMANY)
                    {
                        MethodInfo queryMany = typeof(Query).GetMethod("QueryMany", BindingFlags.Public | BindingFlags.Static).MakeGenericMethod(type);
                        object[] args = new object[4]
                        {
                            db, colname, exp, resultCount
                        };
                        var result = queryMany.Invoke(null, args);
                        if (result != null)
                            resultCount = (uint)args[3];

                        queryResponse = ObjectToByteArray(result);
                    }
                    if (queryResponse != null && queryResponse.Length > 0)
                    {
                        ByteRef qrref = new ByteRef(5 + queryResponse.Length);
                        qrref.Set<byte>(0, 1);
                        qrref.Set<uint>(1, resultCount);
                        qrref.Set<byte[]>(5, queryResponse);
                        return qrref.Get();
                    }
                }
                else if (resultType <= DBRESULTTYPE.INSERTMANY)
                {
                    byte[] InsertData = data.Skip(2).ToArray();
                    if (resultType == DBRESULTTYPE.INSERTONE)
                    {                                                
                        MethodInfo insertOne = typeof(Query).GetMethod("InsertOne", BindingFlags.Public | BindingFlags.Static).MakeGenericMethod(type);
                        insertOne.Invoke(null, new object[] { data.Skip(2).ToArray() });
                    }
                    else if (resultType == DBRESULTTYPE.INSERTMANY)
                    {
                        MethodInfo insertMany = typeof(Query).GetMethod("InsertMany", BindingFlags.Public | BindingFlags.Static).MakeGenericMethod(type);
                        insertMany.Invoke(null, new object[] { data.Skip(2).ToArray() });
                    }
                }

            }

            return response;
        }

        public static bool ResponseHandler(byte[] data, int length)
        {
            return true;
        }

        static void Main(string[] args)
        {
            Logger.SetLoggingLevel(LOGGINGLEVEL.ALL, "DatabaseServer.log");

            dbClient = new MongoClient("mongodb://localhost/OnyxDev?maxPoolSize=100");
            ZMQServer server = new ZMQServer("127.0.0.1", 50505);

            db = dbClient.GetDatabase("onyx");

            server.Listen(RequestHandler);

            Logger.Info("Database Server Ready to Rock!");

            while (running)
            {
                Thread.Sleep(1);
            }
        }
    }
}
