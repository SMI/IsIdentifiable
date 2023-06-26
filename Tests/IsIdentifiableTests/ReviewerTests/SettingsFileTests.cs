using ii;
using NUnit.Framework;
using System;

namespace IsIdentifiable.Tests.ReviewerTests
{
    public class SettingsFileTests
    {
        [Test]
        public void TestCutSettingsFileArgs_NoArgs()
        {
            Assert.IsNull(Program.CutSettingsFileArgs(Array.Empty<string>(), out var result));
            Assert.IsEmpty(result);
        }


        [Test]
        public void TestCutSettingsFileArgs_NoYamlFile()
        {
            Assert.IsNull(Program.CutSettingsFileArgs(new[] { "review", "somefish" }, out var result));
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual("review", result[0]);
            Assert.AreEqual("somefish", result[1]);
        }


        [Test]
        public void TestCutSettingsFileArgs_DashYOnly()
        {
            // missing argument, let CommandLineParser sort them out
            Assert.IsNull(Program.CutSettingsFileArgs(new[] { "-y" }, out var result));
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual("-y", result[0]);
        }

        [Test]
        public void TestCutSettingsFileArgs_YamlOnly()
        {
            Assert.AreEqual("myfile.yaml", Program.CutSettingsFileArgs(new[] { "-y", "myfile.yaml" }, out var result));
            Assert.IsEmpty(result);
        }


        [Test]
        public void TestCutSettingsFileArgs_YamlAfter()
        {
            Assert.AreEqual("myfile.yaml", Program.CutSettingsFileArgs(new[] { "review", "db", "someconstr", "-y", "myfile.yaml" }, out var result));

            Assert.AreEqual(3, result.Length);
            Assert.AreEqual("review", result[0]);
            Assert.AreEqual("db", result[1]);
            Assert.AreEqual("someconstr", result[2]);
        }

        [Test]
        public void TestCutSettingsFileArgs_YamlBefore()
        {
            Assert.AreEqual("myfile.yaml", Program.CutSettingsFileArgs(new[] { "-y", "myfile.yaml", "review", "db", "someconstr" }, out var result));

            Assert.AreEqual(3, result.Length);
            Assert.AreEqual("review", result[0]);
            Assert.AreEqual("db", result[1]);
            Assert.AreEqual("someconstr", result[2]);
        }

        [Test]
        public void TestCutSettingsFileArgs_YamlInMiddle()
        {
            Assert.AreEqual("myfile.yaml", Program.CutSettingsFileArgs(new[] { "review", "-y", "myfile.yaml", "db", "someconstr" }, out var result));

            Assert.AreEqual(3, result.Length);
            Assert.AreEqual("review", result[0]);
            Assert.AreEqual("db", result[1]);
            Assert.AreEqual("someconstr", result[2]);
        }

        [Test]
        public void TestDeserialize_SmiServices_DefaultYaml()
        {
            var f = System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory, "default.yaml");
            FileAssert.Exists(f);

            var opts = Program.Deserialize(f, new System.IO.Abstractions.FileSystem());
            Assert.IsNotNull(opts.IsIdentifiableReviewerOptions);
            Assert.IsNotNull(opts.IsIdentifiableOptions);
        }
    }
}
