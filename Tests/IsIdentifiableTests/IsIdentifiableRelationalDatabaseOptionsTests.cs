using FAnsi;
using IsIdentifiable.Options;
using NUnit.Framework;
using System;
using System.IO.Abstractions.TestingHelpers;

namespace IsIdentifiable.Tests
{
    internal class IsIdentifiableRelationalDatabaseOptionsTests
    {
        private MockFileSystem _fileSystem;

        [SetUp]
        public void SetUp()
        {
            _fileSystem = new MockFileSystem();
        }

        [Test]
        public void TestReturnValueForUsingTargets()
        {
            var opt = new IsIdentifiableRelationalDatabaseOptions();

            // no file no problem
            Assert.AreEqual(0, opt.UpdateConnectionStringsToUseTargets(out var targets, _fileSystem));
            Assert.IsEmpty(targets);

            var ff = "fff.yaml";
            opt.TargetsFile = ff;

            // error code because file does not exist
            Assert.AreEqual(1, opt.UpdateConnectionStringsToUseTargets(out targets, _fileSystem));
            Assert.IsEmpty(targets);

            _fileSystem.File.WriteAllText(ff, @$"");

            // file exists but is empty
            Assert.AreEqual(2, opt.UpdateConnectionStringsToUseTargets(out targets, _fileSystem));
            Assert.IsEmpty(targets);

            _fileSystem.File.WriteAllText(ff, @$"Ahoy ye pirates");

            // file exists and has random garbage in it
            Assert.AreEqual(4, opt.UpdateConnectionStringsToUseTargets(out targets, _fileSystem));
            Assert.IsEmpty(targets);

            _fileSystem.File.WriteAllText(
                ff,
                @$"- Name: MyServer
  ConnectionString: yarg
  DatabaseType: MySql");

            // valid Targets file
            Assert.AreEqual(0, opt.UpdateConnectionStringsToUseTargets(out targets, _fileSystem));
            Assert.AreEqual(1, targets.Count);
        }

        [TestCase("fff", false)]
        [TestCase("MyServer", true)]
        [TestCase("myserver", true)]
        [TestCase(null, false)]
        public void TestUsingTargetNameForConstr(string constr, bool expectToUseTargets)
        {
            var targetConstr = "Server=localhost;Username=root;Password=fff";

            // create a Targets.yaml file with a valid target
            _fileSystem.File.WriteAllText(
                "Targets.yaml",
                @$"- Name: MyServer
  ConnectionString: {targetConstr}
  DatabaseType: MySql");

            var opt = new IsIdentifiableRelationalDatabaseOptions();

            Test(opt, constr, targetConstr, expectToUseTargets,
                (o) => o.DatabaseConnectionString, (o) => o.DatabaseType, (o, v) => o.DatabaseConnectionString = v);

            opt = new IsIdentifiableRelationalDatabaseOptions();

            Test(opt, constr, targetConstr, expectToUseTargets,
                (o) => o.AllowlistConnectionString, (o) => o.AllowlistDatabaseType, (o, v) => o.AllowlistConnectionString = v);

            opt = new IsIdentifiableRelationalDatabaseOptions();

            Test(opt, constr, targetConstr, expectToUseTargets,
                (o) => o.DestinationConnectionString, (o) => o.DestinationDatabaseType, (o, v) => o.DestinationConnectionString = v);
        }

        private void Test(IsIdentifiableRelationalDatabaseOptions opt, string constr, string targetConstr, bool expectToUseTargets,
            Func<IsIdentifiableRelationalDatabaseOptions, string> getter,
            Func<IsIdentifiableRelationalDatabaseOptions, DatabaseType?> getterDbType,
            Action<IsIdentifiableRelationalDatabaseOptions, string> setter)
        {
            // there is 1 target
            opt.UpdateConnectionStringsToUseTargets(out var targets, _fileSystem);
            Assert.AreEqual(1, targets.Count);

            Assert.IsNull(getter(opt));

            setter(opt, constr);
            opt.UpdateConnectionStringsToUseTargets(out _, _fileSystem);

            if (expectToUseTargets)
            {
                Assert.AreEqual(targetConstr, getter(opt));
                Assert.AreEqual(DatabaseType.MySql, getterDbType(opt));
            }
            else
            {
                Assert.AreEqual(constr, getter(opt));

                // the default
                Assert.IsTrue(getterDbType(opt) == DatabaseType.MicrosoftSQLServer || getterDbType(opt) == null);
            }
        }
    }
}
