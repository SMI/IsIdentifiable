using FAnsi;
using IsIdentifiable.Options;
using NUnit.Framework;
using System;
using System.IO;

namespace IsIdentifiable.Tests
{
    internal class IsIdentifiableRelationalDatabaseOptionsTests
    {
        [Test]
        public void TestReturnValueForUsingTargets()
        {
            var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Targets.yaml");

            if (File.Exists(path))
                File.Delete(path);

            var opt = new IsIdentifiableRelationalDatabaseOptions();

            // no file no problem
            Assert.AreEqual(0, opt.UpdateConnectionStringsToUseTargets(out var targets));
            Assert.IsEmpty(targets);

            opt.TargetsFile = "fff.yaml";

            var ff = Path.Combine(TestContext.CurrentContext.WorkDirectory, "fff.yaml");
            
            if (File.Exists(ff))
                File.Delete(ff);

            // error code because file does not exist
            Assert.AreEqual(1, opt.UpdateConnectionStringsToUseTargets(out targets));
            Assert.IsEmpty(targets);

            File.WriteAllText(
          Path.Combine(TestContext.CurrentContext.WorkDirectory, "fff.yaml"),

                @$"");

            // file exists but is empty
            Assert.AreEqual(2, opt.UpdateConnectionStringsToUseTargets(out targets));
            Assert.IsEmpty(targets);



            File.WriteAllText(
          Path.Combine(TestContext.CurrentContext.WorkDirectory, "fff.yaml"),

                @$"Ahoy ye pirates");

            // file exists and has random garbage in it
            Assert.AreEqual(4, opt.UpdateConnectionStringsToUseTargets(out targets));
            Assert.IsEmpty(targets);



            File.WriteAllText(
          Path.Combine(TestContext.CurrentContext.WorkDirectory, "fff.yaml"),


                @$"- Name: MyServer
  ConnectionString: yarg
  DatabaseType: MySql");

            // valid Targets file
            Assert.AreEqual(0, opt.UpdateConnectionStringsToUseTargets(out targets));
            Assert.AreEqual(1,targets.Count);
        }

        [TestCase("fff",false)]
        [TestCase("MyServer", true)]
        [TestCase("myserver", true)]
        [TestCase(null, false)]
        public void TestUsingTargetNameForConstr(string constr,bool expectToUseTargets)
        {
            string targetConstr = "Server=localhost;Username=root;Password=fff";

            // create a Targets.yaml file with a valid target
            File.WriteAllText(
                
                Path.Combine(TestContext.CurrentContext.WorkDirectory,"Targets.yaml"),

                @$"- Name: MyServer
  ConnectionString: {targetConstr}
  DatabaseType: MySql");

            var opt = new IsIdentifiableRelationalDatabaseOptions();

            Test(opt,constr, targetConstr,expectToUseTargets, 
                (o) => o.DatabaseConnectionString, (o) => o.DatabaseType, (o, v) => o.DatabaseConnectionString = v);
            
            opt = new IsIdentifiableRelationalDatabaseOptions();

            Test(opt, constr, targetConstr, expectToUseTargets, 
                (o) => o.AllowlistConnectionString, (o) => o.AllowlistDatabaseType, (o, v) => o.AllowlistConnectionString = v);

            opt = new IsIdentifiableRelationalDatabaseOptions();

            Test(opt, constr, targetConstr, expectToUseTargets, 
                (o) => o.DestinationConnectionString, (o) => o.DestinationDatabaseType, (o, v) => o.DestinationConnectionString = v);
        }

        private void Test(IsIdentifiableRelationalDatabaseOptions opt, string constr, string targetConstr, bool expectToUseTargets,
            Func<IsIdentifiableRelationalDatabaseOptions,string> getter,
            Func<IsIdentifiableRelationalDatabaseOptions, DatabaseType?> getterDbType,
            Action<IsIdentifiableRelationalDatabaseOptions, string> setter)
        {
            // there is 1 target
            opt.UpdateConnectionStringsToUseTargets(out var targets);
            Assert.AreEqual(1, targets.Count);

            Assert.IsNull(getter(opt));

            setter(opt, constr);
            opt.UpdateConnectionStringsToUseTargets(out _);

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
