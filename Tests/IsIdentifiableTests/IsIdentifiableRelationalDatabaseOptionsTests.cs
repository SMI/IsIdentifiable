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

            Assert.Multiple(() =>
            {
                // no file no problem
                Assert.That(opt.UpdateConnectionStringsToUseTargets(out var targets, _fileSystem), Is.EqualTo(0));
                Assert.That(targets, Is.Empty);
            });

            var ff = "fff.yaml";
            opt.TargetsFile = ff;

            Assert.Multiple(() =>
            {
                // error code because file does not exist
                Assert.That(opt.UpdateConnectionStringsToUseTargets(out var targets, _fileSystem), Is.EqualTo(1));
                Assert.That(targets, Is.Empty);
            });

            _fileSystem.File.WriteAllText(ff, @$"");

            Assert.Multiple(() =>
            {
                // file exists but is empty
                Assert.That(opt.UpdateConnectionStringsToUseTargets(out var targets, _fileSystem), Is.EqualTo(2));
                Assert.That(targets, Is.Empty);
            });

            _fileSystem.File.WriteAllText(ff, @$"Ahoy ye pirates");

            Assert.Multiple(() =>
            {
                // file exists and has random garbage in it
                Assert.That(opt.UpdateConnectionStringsToUseTargets(out var targets, _fileSystem), Is.EqualTo(4));
                Assert.That(targets, Is.Empty);
            });

            _fileSystem.File.WriteAllText(
                ff,
                @$"- Name: MyServer
  ConnectionString: yarg
  DatabaseType: MySql");

            Assert.Multiple(() =>
            {
                // valid Targets file
                Assert.That(opt.UpdateConnectionStringsToUseTargets(out var targets, _fileSystem), Is.EqualTo(0));
                Assert.That(targets, Has.Count.EqualTo(1));
            });
        }

        [TestCase("fff", false)]
        [TestCase("MyServer", true)]
        [TestCase("myserver", true)]
        [TestCase(null, false)]
        public void TestUsingTargetNameForConstr(string? constr, bool expectToUseTargets)
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
            Assert.Multiple(() =>
            {
                Assert.That(targets, Has.Count.EqualTo(1));

                Assert.That(getter(opt), Is.Null);
            });

            setter(opt, constr);
            opt.UpdateConnectionStringsToUseTargets(out _, _fileSystem);

            if (expectToUseTargets)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(getter(opt), Is.EqualTo(targetConstr));
                    Assert.That(getterDbType(opt), Is.EqualTo(DatabaseType.MySql));
                });
            }
            else
            {
                Assert.Multiple(() =>
                {
                    Assert.That(getter(opt), Is.EqualTo(constr));

                    // the default
                    Assert.That(getterDbType(opt) == DatabaseType.MicrosoftSQLServer || getterDbType(opt) == null, Is.True);
                });
            }
        }
    }
}
