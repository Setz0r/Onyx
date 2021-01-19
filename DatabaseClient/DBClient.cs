using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Data.Game;
using Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serialize.Linq.Extensions;
using Serialize.Linq.Nodes;
using Serialize.Linq.Serializers;
using Toolbelt;

namespace DatabaseClient
{
    public enum DBREQUESTTYPE : byte
    {
        ACCOUNT = 1,
        PLAYER = 2,
        ACTIVESESSION = 3
    }

    public enum DBRESULTTYPE : byte
    {
        GETONE      = 0,
        GETMANY     = 1,
        INSERTONE   = 2,
        INSERTMANY  = 3,
        UPDATEONE   = 4,
        UPDATEMANY  = 5,
        DELETEONE   = 6,
        DELETEMANY  = 7,
        GETCOUNT    = 8,
        GETMAXID    = 9
    }

    public enum DBRESPONSETYPE : byte
    {
        FAILURE = 0,
        SUCCESS = 1
    }

    public static class DBClient
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

        public static T GetOne<T>(DBREQUESTTYPE type, Expression<Func<T, bool>> expression) where T : IQueryResult, new()
        {
            var serializer = new ExpressionSerializer(new BinarySerializer());
            byte[] query = serializer.SerializeBinary(expression);

            ByteRef payload = new ByteRef(2 + query.Length);
            payload.Set<byte>(0, (byte)type);
            payload.Set<byte>(1, (byte)DBRESULTTYPE.GETONE);
            payload.Set<byte[]>(2, query);

            ByteRef queryResult = new ByteRef(SendQuery(payload.Get()));            

            if (queryResult.Length == 1 || (DBRESPONSETYPE)queryResult.GetByte(0) == DBRESPONSETYPE.FAILURE || 
                (queryResult.Length == 2 && queryResult.GetByte(1) == 0) || 
                (queryResult.Length >= 5 && queryResult.GetUInt32(1) == 0))
                return default(T);
            
            T result = Deserialize<T>(queryResult.Get().Skip(5).ToArray());

            return result;
        }

        public static List<T> GetMany<T>(DBREQUESTTYPE type, Expression<Func<T, bool>> expression) where T : IQueryResult, new()
        {            
            var serializer = new ExpressionSerializer(new BinarySerializer());
            byte[] query = serializer.SerializeBinary(expression);

            ByteRef payload = new ByteRef(2 + query.Length);
            payload.Set<byte>(0, (byte)type);
            payload.Set<byte>(1, (byte)DBRESULTTYPE.GETMANY);
            payload.Set<byte[]>(2, query);

            ByteRef queryResult = new ByteRef(SendQuery(payload.Get()));

            if (queryResult.Length <= 1 || (DBRESPONSETYPE)queryResult.GetByte(0) == DBRESPONSETYPE.FAILURE || queryResult.GetUInt32(1) == 0)
                return default(List<T>);

            List<T> result = Deserialize<List<T>>(queryResult.Get().Skip(5).ToArray());

            return result;
        }

        public static bool InsertOne<T>(DBREQUESTTYPE type, T obj)
        {
            byte[] serializedObj = Serialize(obj);
            ByteRef payload = new ByteRef(2 + serializedObj.Length);
            payload.Set<byte>(0, type);
            payload.Set<byte>(1, (byte)DBRESULTTYPE.INSERTONE);
            payload.Set<byte[]>(2, serializedObj);

            byte[] queryResult = SendQuery(payload.Get());

            if (queryResult.Length == 1 || (DBRESPONSETYPE)queryResult[0] == DBRESPONSETYPE.FAILURE)
                return false;

            return true;
        }

        public static bool InsertMany<T>(DBREQUESTTYPE type, List<T> obj)
        {
            byte[] serializedObj = Serialize(obj);
            ByteRef payload = new ByteRef(2 + serializedObj.Length);
            payload.Set<byte>(0, type);
            payload.Set<byte>(1, (byte)DBRESULTTYPE.INSERTMANY);
            payload.Set<byte[]>(2, serializedObj);

            byte[] queryResult = SendQuery(payload.Get());

            if (queryResult.Length == 1 || (DBRESPONSETYPE)queryResult[0] == DBRESPONSETYPE.FAILURE)
                return false;

            return true;
        }

        public static long UpdateOne<T>(DBREQUESTTYPE type, Expression<Func<T, bool>> expression, Dictionary<string,object> update)
        {
            long updated = 0;
            var serializer = new ExpressionSerializer(new BinarySerializer());
            string json = JsonConvert.SerializeObject(update);
            byte[] serializedFilter = serializer.SerializeBinary(expression);
            byte[] serializedUpdate = Serialize(json);
            ByteRef payload = new ByteRef(2 + 4 + serializedFilter.Length + 4 + serializedUpdate.Length);
            payload.Set<byte>(0, type);
            payload.Set<byte>(1, (byte)DBRESULTTYPE.UPDATEONE);
            payload.Set<int>(2, serializedFilter.Length);
            payload.Set<byte[]>(6, serializedFilter);
            payload.Set<int>(6 + serializedFilter.Length, serializedUpdate.Length);
            payload.Set<byte[]>(6 + serializedFilter.Length + 4, serializedUpdate);

            ByteRef queryResult = new ByteRef(SendQuery(payload.Get()));

            if (queryResult.Length >= 5 || (DBRESPONSETYPE)queryResult.GetByte(0) != DBRESPONSETYPE.FAILURE)
                updated = queryResult.GetUInt32(1);
            
            return updated;
        }

        public static long UpdateMany<T>(DBREQUESTTYPE type, Expression<Func<T, bool>> expression, Dictionary<string, object> update)
        {
            long updated = 0;
            var serializer = new ExpressionSerializer(new BinarySerializer());
            byte[] serializedFilter = serializer.SerializeBinary(expression);
            byte[] serializedUpdate = Serialize(update);
            ByteRef payload = new ByteRef(2 + 4 + serializedFilter.Length + 4 + serializedUpdate.Length);
            payload.Set<byte>(0, type);
            payload.Set<byte>(1, (byte)DBRESULTTYPE.UPDATEMANY);
            payload.Set<int>(2, serializedFilter.Length);
            payload.Set<byte[]>(6, serializedFilter);
            payload.Set<int>(6 + serializedFilter.Length, serializedUpdate.Length);
            payload.Set<byte[]>(6 + serializedFilter.Length + 4, serializedUpdate);

            ByteRef queryResult = new ByteRef(SendQuery(payload.Get()));

            if (queryResult.Length >= 5 || (DBRESPONSETYPE)queryResult.GetByte(0) != DBRESPONSETYPE.FAILURE)
                updated = queryResult.GetUInt32(1);

            return updated;
        }

        public static long DeleteOne<T>(DBREQUESTTYPE type, Expression<Func<T, bool>> expression)
        {
            long deleted = 0;
            var serializer = new ExpressionSerializer(new BinarySerializer());
            byte[] query = serializer.SerializeBinary(expression);

            ByteRef payload = new ByteRef(2 + query.Length);
            payload.Set<byte>(0, (byte)type);
            payload.Set<byte>(1, (byte)DBRESULTTYPE.DELETEONE);
            payload.Set<byte[]>(2, query);

            ByteRef queryResult = new ByteRef(SendQuery(payload.Get()));

            if (queryResult.Length >= 5 || (DBRESPONSETYPE)queryResult.GetByte(0) != DBRESPONSETYPE.FAILURE)
                deleted = queryResult.GetUInt32(1);

            return deleted;
        }

        public static long DeleteMany<T>(DBREQUESTTYPE type, Expression<Func<T, bool>> expression)
        {
            long deleted = 0;
            var serializer = new ExpressionSerializer(new BinarySerializer());
            byte[] query = serializer.SerializeBinary(expression);

            ByteRef payload = new ByteRef(2 + query.Length);
            payload.Set<byte>(0, (byte)type);
            payload.Set<byte>(1, (byte)DBRESULTTYPE.DELETEMANY);
            payload.Set<byte[]>(2, query);

            ByteRef queryResult = new ByteRef(SendQuery(payload.Get()));

            if (queryResult.Length >= 5 || (DBRESPONSETYPE)queryResult.GetByte(0) != DBRESPONSETYPE.FAILURE)
                deleted = queryResult.GetUInt32(1);

            return deleted;
        }

        public static uint GetMaxID(DBREQUESTTYPE type)
        {
            uint maxID = 0;

            ByteRef payload = new ByteRef(2);
            payload.Set<byte>(0, (byte)type);
            payload.Set<byte>(1, (byte)DBRESULTTYPE.GETMAXID);

            ByteRef queryResult = new ByteRef(SendQuery(payload.Get()));

            if (queryResult.Length >= 5 && (DBRESPONSETYPE)queryResult.GetByte(0) != DBRESPONSETYPE.FAILURE)
                maxID = queryResult.GetUInt32(1);

            return maxID;
        }

        public static byte[] SendQuery(byte[] payload)
        {
            ByteRef response = new ByteRef(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00 });
            TcpClient client = null;

            //bool success = false;
            try
            {
                client = new TcpClient("127.0.0.1", 50505);
            }
            catch (SocketException e)
            {
                Logger.Error("Unable to connect to Database Server : {0}", new object[] { e.Message });
                Environment.Exit(0);
            }

            NetworkStream stream = client.GetStream();

            stream.Write(payload, 0, payload.Length);

            bool receivedData = false;
            while (!receivedData)
            {
                if (stream.CanRead)
                {
                    response.Resize(0);
                    int bytesRead = 0;
                    byte[] readbuffer = new byte[4096];
                    if (stream.DataAvailable)
                    do
                    {
                        bytesRead = stream.Read(readbuffer, 0, readbuffer.Length);
                        response.Append(readbuffer.Take(bytesRead).ToArray());
                    } while (stream.DataAvailable && bytesRead > 0);
                    if (response.Length > 0)
                        receivedData = true;
                    //success = true;
                }
            }
            //if (client.Connect())
            //{
            //    int retryCounter = 3;

            //    while (success == false && retryCounter > 0)
            //    {
            //        try
            //        {
            //            response = client.SendData(payload);
            //            if (response != null && response.Length > 0)
            //            {
            //                success = true;
            //            }
            //            else
            //            {
            //                retryCounter--;
            //            }
            //        }
            //        catch(Exception e)
            //        {
            //            Logger.Error("Error trying to query results from database server : {0}", new object[] { e.Message });
            //        }
            //    }
            //}
            client.Client.Disconnect(true);
            client.Dispose();
            return response.Get();
        }

        public struct TestData
        {
            public int id;
            public string name;
        }

        public static void TestSerialize()
        {
            List<Data.Game.Account> accounts = GetMany<Data.Game.Account>(DBREQUESTTYPE.ACCOUNT, p => p.Username.StartsWith("dude") || p.AccountId == 1);
            //List<Account> accounts = GetMany<Account>(DBREQUESTTYPE.ACCOUNT, p => p.username == "dude" || p.id == 1).OrderBy(x => x.id).ToList();
            if (accounts == null)
            {
                Console.WriteLine("failure");
            } else
            {
                Console.WriteLine("Success : " + accounts.Count.ToString());
            }

            //TestData[] data = new TestData[]
            //{
            //    new TestData() { id = 1, name = "setzor" },
            //    new TestData() { id = 2, name = "dude" }
            //};

            //Expression<Func<TestData, bool>> expression = p => p.name == "dude" || p.id == 1;

            //var serializer = new ExpressionSerializer(new BinarySerializer());

            //byte[] query = serializer.SerializeBinary(expression);

            //var exp = serializer.DeserializeBinary(query);

            //ExpressionNode node = exp.ToExpressionNode();

            //var e = node.ToBooleanExpression<TestData>();

            //List<TestData> mydata = data.Where(e.Compile()).ToList();

            //Console.WriteLine(query);

        }
    }
}
