using IsIdentifiable.Options;
using IsIdentifiable.Rules;
using IsIdentifiable.Runners;
using MongoDB.Bson;
using MongoDB.Driver;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private MongoClient GetMongoClient()
        {

            MongoClientSettings address = GetMongoClientSettings();

            TestContext.Out.WriteLine("Checking the following configuration:" + Environment.NewLine + address);

            var client = new MongoClient(address);

            try
            {
                using IAsyncCursor<BsonDocument> _ = client.ListDatabases();
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

            return client;
        }
        [Test]
        public void TestConnectingToMongoDb()
        {
            var client = GetMongoClient();
            var cursor = client.ListDatabases();
            cursor.Dispose();
            
        }


        static object[] TestDocuments =
        {
            new object[] { "{Name: \"hello\"}","Name", "[a-z]+",1},
        };


        /// <summary>
        /// Tests picking up identifiable data in root level or sublevel tags of MongoDb documents
        /// </summary>
        /// <param name="json">The full JSON to insert into the MongoDb database</param>
        /// <param name="column">The tag name you want the <paramref name="regex"/> applied to to detect the problem data or null for 'match any tag'</param>
        /// <param name="regex">The pattern that should be applied to match identifiable data.  In a real world scenario this wouldn't be needed because
        /// this function would come from NLP or existing rules base</param>
        /// <param name="expectedMatches">The number of reports you expect to be raised</param>
        [TestCaseSource(nameof(TestDocuments))]
        public void TestAnalyseIdentifiableDataInMongoDb(string json, string column, string regex, int expectedMatches)
        {
            const string collectionName = "SomeData";
            const string databaseName = "IsIdentifiableTests";

            var client = GetMongoClient();
            var db = client.GetDatabase(databaseName);

            // remove any old data from previous test runs
            db.DropCollection(collectionName);
            db.CreateCollection(collectionName);

            BsonDocument doc = BsonDocument.Parse(json);

            // Insert document into database
            IMongoCollection<BsonDocument> collection =
                db.GetCollection<BsonDocument>(collectionName);

            BulkWriteResult<BsonDocument> res = collection.BulkWrite(new[] {
                doc
            }.Select(d => new InsertOneModel<BsonDocument>(d)));

            Assert.IsTrue(res.IsAcknowledged);

            var read = collection.WithReadConcern(ReadConcern.Available);
            Assert.AreEqual(1, read.CountDocuments((d)=>true), "There should be exactly 1 document in the collection");

            var settings = GetMongoClientSettings();

            var runner = new MongoRunner(new IsIdentifiableMongoOptions
            {
                DatabaseName = databaseName,
                CollectionName = collectionName,
                MongoConnectionString = $"mongodb://{settings.Server}",
                StoreReport = true
            }) ;

            runner.CustomRules.Add(new IsIdentifiableRule
            {
                Action = RuleAction.Report,
                As = Failures.FailureClassification.Person,
                IfColumn = column,
                IfPattern = regex
            });

            Assert.AreEqual(0,runner.Run(),"MongoDb runner returned a non zero exit code. Indicating failure");

            Assert.AreEqual(expectedMatches, runner.CountOfFailureParts, "The number of problem parts identified by IsIdentifiable did not match test case expectations");
            
        }
    }
}
