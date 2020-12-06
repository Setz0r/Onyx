using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Data.Game;
using Networking;
using Serialize.Linq.Extensions;
using Serialize.Linq.Nodes;
using Serialize.Linq.Serializers;
using Toolbelt;

namespace DatabaseClient
{
    public enum DBREQUESTTYPE : byte
    {
        ACCOUNT = 1,
        PLAYER = 2
    }

    public enum DBRESULTTYPE : byte
    {
        GETONE = 0,
        GETMANY = 1,
        INSERTONE = 2,
        INSERTMANY = 3,
        GETCOUNT = 4,
        GETMAXID = 5
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

        public static T GetOne<T>(DBREQUESTTYPE type, Expression<Func<T, bool>> expression) where T : IQueryResult, new()
        {
            var serializer = new ExpressionSerializer(new BinarySerializer());
            byte[] query = serializer.SerializeBinary(expression);

            ByteRef payload = new ByteRef(2 + query.Length);
            payload.Set<byte>(0, (byte)type);
            payload.Set<byte>(1, (byte)DBRESULTTYPE.GETONE);
            payload.Set<byte[]>(2, query);

            byte[] queryResult = SendQuery(payload.Get());            

            if (queryResult.Length == 1 || (DBRESPONSETYPE)queryResult[0] == DBRESPONSETYPE.FAILURE || queryResult[1] == 0)
                return default(T);
            
            T result = Deserialize<T>(queryResult.Skip(2).ToArray());

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

        public static byte[] SendQuery(byte[] payload)
        {
            byte[] response = null;
            bool success = false;

            ZMQClient client = new ZMQClient("127.0.0.1", 50505);
            if (client.Connect())
            {
                int retryCounter = 3;

                while (success == false && retryCounter > 0)
                {
                    try
                    {
                        response = client.SendData(payload);
                        if (response != null && response.Length > 0)
                        {
                            success = true;
                        }
                        else
                        {
                            retryCounter--;
                        }
                    }
                    catch(Exception e)
                    {
                        Logger.Error("Error trying to query results from database server : {0}", new object[] { e.Message });
                    }
                }
            }
            if (success == false)
            {
                response = new byte[1] { 0 };
            }

            return response;
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
