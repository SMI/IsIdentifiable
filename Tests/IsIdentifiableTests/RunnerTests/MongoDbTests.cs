using MongoDB.Bson;
using MongoDB.Driver;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace IsIdentifiable.Tests.RunnerTests
{
    internal class MongoDbTests
    {
        public static MongoClientSettings GetMongoClientSettings()
        {
            IDeserializer deserializer = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .Build();

            return deserializer.Deserialize<A>(new StreamReader(Path.Combine(TestContext.CurrentContext.TestDirectory, "Mongo.yaml")));
        }

        class A : MongoClientSettings
        {
            private string _host;
            private int _port;

            public string Host
            {
                get => _host;
                set
                {
                    _host = value;
                    Server = new MongoServerAddress(_host, _port);
                }
            }

            public int Port
            {
                get => _port;
                set
                {
                    _port = value;
                    Server = new MongoServerAddress(_host, _port);
                }
            }

            public A()
            {

                DirectConnection = true;
                ConnectTimeout = new TimeSpan(0, 0, 0, 5);
                SocketTimeout = new TimeSpan(0, 0, 0, 5);
                HeartbeatTimeout = new TimeSpan(0, 0, 0, 5);
                ServerSelectionTimeout = new TimeSpan(0, 0, 0, 5);
                WaitQueueTimeout = new TimeSpan(0, 0, 05);
            }
            public override string ToString()
            {
                return Host + ":" + Port;
            }
        }
      
        [Test]
        public void TestConnectingToMongoDb()
        {
            MongoClientSettings address = GetMongoClientSettings();

            TestContext.Out.WriteLine("Checking the following configuration:" + Environment.NewLine + address);

            var client = new MongoClient(address);

            try
            {
                IAsyncCursor<BsonDocument> _ = client.ListDatabases();
            }
            catch (Exception e)
            {
                string msg =
                    e is MongoNotPrimaryException
                    ? "Connected to non-primary MongoDB server. Check replication is enabled"
                    : $"Could not connect to MongoDB at {address}";

                msg += $": {e}";
                
                Assert.Fail(msg);
            }
        }
    }
}
