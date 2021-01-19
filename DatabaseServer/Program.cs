using System;
using Networking;
using MongoDB.Bson;
using MongoDB.Driver.Linq;
using MongoDB.Driver;
using System.Threading;
using Toolbelt;
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
using System.Diagnostics;
using System.Net;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Data.Game.Entities;

namespace DatabaseServer
{
    class Program
    {
        public static MongoClient dbClient;
        public static IMongoDatabase db;
        public static bool running = true;

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
                Logger.Error("Unable to deserialize data received from Database Server : {0}", new object[] { e.Message });
                return default(T);
            }
        }

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
                case DBREQUESTTYPE.PLAYER:
                    return (typeof(Player), "players");
                case DBREQUESTTYPE.ACTIVESESSION:
                    return (typeof(ActiveSession), "activesessions");
            }

            return (null, "");
        }

        private static int DatabaseConnectHandler(SessionTcpClient client)
        {
            var addr = ((IPEndPoint)client.Client.RemoteEndPoint).Address;
            var port = ((IPEndPoint)client.Client.RemoteEndPoint).Port;
            Logger.Info("Database Server Connection : {0} : {1}", new object[] { addr.ToString(), port });


            return 1;
        }

        private static int DatabaseDataHandler(SessionTcpClient client, Byte[] data, int length)
        {
            byte[] response = new byte[] { 0 };
            DBREQUESTTYPE requestType;
            DBRESULTTYPE resultType;

            //Console.WriteLine("Request handler fired : " + length.ToString());
            //Logger.Info("Threads in Use: {0}, Total Memory: {1}", new object[] { Process.GetCurrentProcess().Threads.Count, GC.GetTotalMemory(false) });
            if (length >= 2)
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
                    var exp = serializer.DeserializeBinary(data.Skip(2).Take(length-2).ToArray());

                    if (resultType == DBRESULTTYPE.GETONE)
                    {
                        MethodInfo queryOne = typeof(Query).GetMethod("QueryOne", BindingFlags.Public | BindingFlags.Static).MakeGenericMethod(type);
                        var result = queryOne.Invoke(null, new object[] { db, colname, exp });
                        if (result != null)
                        {
                            resultCount = 1;
                            queryResponse = ObjectToByteArray(result);
                        }
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
                        {
                            resultCount = (uint)args[3];
                            queryResponse = ObjectToByteArray(result);
                        }
                    }
                    if (queryResponse != null && queryResponse.Length > 0)
                    {
                        ByteRef qrref = new ByteRef(5 + queryResponse.Length);
                        qrref.Set<byte>(0, 1);
                        qrref.Set<uint>(1, resultCount);
                        qrref.Set<byte[]>(5, queryResponse);
                        client.Client.Send(qrref.Get());
                        return 1;
                    }
                    else
                    {
                        ByteRef qrref = new ByteRef(2);
                        qrref.Set<byte>(0, 1);
                        qrref.Set<uint>(1, 0);
                        client.Client.Send(qrref.Get());
                        return 1;
                    }
                }
                else if (resultType <= DBRESULTTYPE.INSERTMANY)
                {
                    byte[] InsertData = data.Skip(2).ToArray();
                    if (resultType == DBRESULTTYPE.INSERTONE)
                    {                                                
                        MethodInfo insertOne = typeof(Query).GetMethod("InsertOne", BindingFlags.Public | BindingFlags.Static).MakeGenericMethod(type);
                        insertOne.Invoke(null, new object[] { db, colname, InsertData });
                    }
                    else if (resultType == DBRESULTTYPE.INSERTMANY)
                    {
                        MethodInfo insertMany = typeof(Query).GetMethod("InsertMany", BindingFlags.Public | BindingFlags.Static).MakeGenericMethod(type);
                        insertMany.Invoke(null, new object[] { db, colname, InsertData });
                    }
                }
                else if (resultType <= DBRESULTTYPE.UPDATEMANY)
                {
                    uint updated = 0;
                    ByteRef dataBR = new ByteRef(data);

                    int filterLength = dataBR.GetInt32(2);
                    byte[] filterBytes = dataBR.GetBytes(6, filterLength);
                    int updateLength = dataBR.GetInt32(6 + filterLength);
                    byte[] updateBytes = dataBR.GetBytes(6 + filterLength + 4, updateLength);

                    var json = Deserialize<string>(updateBytes);

                    var serializer = new ExpressionSerializer(new BinarySerializer());
                    var exp = serializer.DeserializeBinary(filterBytes);

                    Dictionary<string, object> update = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                    
                    if (resultType == DBRESULTTYPE.UPDATEONE)
                    {                        
                        MethodInfo updateOne = typeof(Query).GetMethod("UpdateOne", BindingFlags.Public | BindingFlags.Static).MakeGenericMethod(type);
                        updated = Convert.ToUInt32(updateOne.Invoke(null, new object[] { db, colname, exp, update }));
                    }
                    else if (resultType == DBRESULTTYPE.UPDATEMANY)
                    {
                        MethodInfo updateMany = typeof(Query).GetMethod("UpdateMany", BindingFlags.Public | BindingFlags.Static).MakeGenericMethod(type);
                        updated = Convert.ToUInt32(updateMany.Invoke(null, new object[] { db, colname, exp, update }));
                    }

                    ByteRef qrref = new ByteRef(5);
                    qrref.Set<byte>(0, 1);
                    qrref.Set<uint>(1, updated);
                    client.Client.Send(qrref.Get());
                    return 1;
                }
                else if (resultType <= DBRESULTTYPE.DELETEMANY)
                {
                    var serializer = new ExpressionSerializer(new BinarySerializer());
                    var exp = serializer.DeserializeBinary(data.Skip(2).Take(length - 2).ToArray());
                    long deleted = 0;
                    if (resultType == DBRESULTTYPE.DELETEONE)
                    {
                        MethodInfo queryOne = typeof(Query).GetMethod("DeleteOne", BindingFlags.Public | BindingFlags.Static).MakeGenericMethod(type);
                        deleted = Convert.ToUInt32(queryOne.Invoke(null, new object[] { db, colname, exp }));
                    }
                    else if (resultType == DBRESULTTYPE.DELETEMANY)
                    {
                        MethodInfo queryOne = typeof(Query).GetMethod("DeleteMany", BindingFlags.Public | BindingFlags.Static).MakeGenericMethod(type);
                        deleted = Convert.ToUInt32(queryOne.Invoke(null, new object[] { db, colname, exp }));
                    }

                    ByteRef qrref = new ByteRef(5);
                    qrref.Set<byte>(0, 1);
                    qrref.Set<uint>(1, deleted);
                    client.Client.Send(qrref.Get());
                    return 1;
                }
                else if (resultType <= DBRESULTTYPE.GETMAXID)
                {
                    uint maxID = 0;

                    if (requestType == DBREQUESTTYPE.ACCOUNT)
                    {                        
                        maxID = Query.GetMaxAccountID(db, colname);
                    }
                    else if (requestType == DBREQUESTTYPE.PLAYER)
                    {
                        maxID = Query.GetMaxPlayerID(db, colname);
                    }

                    ByteRef qrref = new ByteRef(5);
                    qrref.Set<byte>(0, 1);
                    qrref.Set<uint>(1, maxID);
                    client.Client.Send(qrref.Get());
                    return 1;
                }

            }
            client.Client.Send(response);
            return 0;
        }

        private static int DatabaseDisconnectHandler(SessionTcpClient client)
        {
            if (!client.IsDead)
            {
                var addr = ((IPEndPoint)client.Client.RemoteEndPoint).Address;
                var port = ((IPEndPoint)client.Client.RemoteEndPoint).Port;
                client.Client.Disconnect(true);
                Logger.Info("Database Server Disconnection : {0} : {1}", new object[] { addr.ToString(), port });
            }
            return 1;
        }


        static void Main(string[] args)
        {
            Logger.SetLoggingLevel(LOGGINGLEVEL.ALL);

            // TODO: Make this a configuration item

            dbClient = new MongoClient("mongodb://localhost/OnyxDev?maxPoolSize=1000");
            TCPServer server = new TCPServer("Database Server", "127.0.0.1", 50505, DatabaseConnectHandler, DatabaseDataHandler, DatabaseDisconnectHandler);

            db = dbClient.GetDatabase("onyx");

            Logger.Info("Database Server Ready to Rock!");

            while (running)
            {
                Thread.Sleep(1);
                //GC.Collect();
            }
        }
    }
}
