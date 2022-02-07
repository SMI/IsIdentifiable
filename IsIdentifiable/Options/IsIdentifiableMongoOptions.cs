using System;
using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace IsIdentifiable.Options
{
    /// <summary>
    /// Options for connecting to a mongodb database and evaluating the identifiability of a
    /// document collection.
    /// </summary>
    [Verb("mongo", HelpText = "Run tool on contents of a MongoDb document collection")]
    public class IsIdentifiableMongoOptions : IsIdentifiableDicomOptions
    {
        /// <summary>
        /// The MongoDB hostname or IP.  Defaults to localhost
        /// </summary>
        [Option('h', "mongo-host", Required = false, HelpText = "The MongoDB hostname or IP", Default = "localhost")]
        public string HostName { get; set; }

        /// <summary>
        /// The port MongoDb is running on.  Defaults to 27017
        /// </summary>
        [Option('p', "mongo-port", Required = false, HelpText = "The MongoDB port", Default = 27017)]
        public int Port { get; set; }

        /// <summary>
        /// The database to connect to
        /// </summary>
        [Option('d',"db", Required = true, HelpText = "The database to scan")]
        public string DatabaseName { get; set; }

        /// <summary>
        /// The name of the collection which should be queried
        /// </summary>

        [Option('l',"coll", Required = true, HelpText = "The collection to scan")]
        public string CollectionName { get; set; }

        /// <summary>
        /// The query to run against the <see cref="CollectionName"/> to extract a subset of records
        /// to evaluate.  Can be null.
        /// </summary>

        [Option('q', "query-file", Required = false, HelpText = "The file to build the reprocessing query from")]
        public string QueryFile { get; set; }

        /// <summary>
        /// The batch size to set on the MongoDb fetch query
        /// </summary>
        [Option("batch-size", Required = false, HelpText = "The batch size to query MongoDB with, if specified", Default = 0)]
        public int MongoDbBatchSize { get; set; }


        /// <summary>
        /// If set will use the max. number of threads available, otherwise defaults to half the available threads
        /// </summary>
        [Option(HelpText = "If set will use the max. number of threads available, otherwise defaults to half the available threads")]
        public bool UseMaxThreads { get; set; }

        /// <summary>
        /// Command line examples for running IsIdentifiable on mongodb
        /// </summary>
        [Usage]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("Run on a MongoDB database", new IsIdentifiableMongoOptions
                {
                    HostName = "localhost",
                    Port = 1234,
                    DatabaseName = "dicom",
                    CollectionName = "images",
                    MongoDbBatchSize = 5000
                });
            }
        }

        /// <summary>
        /// Returns a string indicating that MongoDb is the data format and the database
        /// name being queried along with the name of the collection.  Does not include any
        /// reference to <see cref="QueryFile"/>
        /// </summary>
        /// <returns></returns>
        public override string GetTargetName()
        {
            return "MongoDB-" + DatabaseName + "-" + CollectionName + "-";
        }

        /// <summary>
        /// Validates that the options are valid for this destination. Throws Exception if they
        /// are invalid.
        /// </summary>
        /// <exception cref="Exception"></exception>
        public override void ValidateOptions()
        {
            base.ValidateOptions();

            if (ColumnReport)
                throw new Exception("ColumnReport can't be generated from a MongoDB source");
        }
    }
}
