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
            Assert.Multiple(() =>
            {
                Assert.That(Program.CutSettingsFileArgs(Array.Empty<string>(), out var result), Is.Null);
                Assert.That(result, Is.Empty);
            });
        }


        [Test]
        public void TestCutSettingsFileArgs_NoYamlFile()
        {
            Assert.Multiple(static () =>
            {
                Assert.That(Program.CutSettingsFileArgs(new[] { "review", "somefish" }, out var result), Is.Null);
                Assert.That(result, Has.Length.EqualTo(2));
                Assert.That(result[0], Is.EqualTo("review"));
                Assert.That(result[1], Is.EqualTo("somefish"));
            });
        }


        [Test]
        public void TestCutSettingsFileArgs_DashYOnly()
        {
            Assert.Multiple(static () =>
            {
                // missing argument, let CommandLineParser sort them out
                Assert.That(Program.CutSettingsFileArgs(new[] { "-y" }, out var result), Is.Null);
                Assert.That(result, Has.Length.EqualTo(1));
                Assert.That(result[0], Is.EqualTo("-y"));
            });
        }

        [Test]
        public void TestCutSettingsFileArgs_YamlOnly()
        {
            Assert.Multiple(static () =>
            {
                Assert.That(Program.CutSettingsFileArgs(new[] { "-y", "myfile.yaml" }, out var result), Is.EqualTo("myfile.yaml"));
                Assert.That(result, Is.Empty);
            });
        }


        [Test]
        public void TestCutSettingsFileArgs_YamlAfter()
        {
            Assert.Multiple(static () =>
            {
                Assert.That(Program.CutSettingsFileArgs(new[] { "review", "db", "someconstr", "-y", "myfile.yaml" }, out var result), Is.EqualTo("myfile.yaml"));

                Assert.That(result, Has.Length.EqualTo(3));
                Assert.That(result[0], Is.EqualTo("review"));
                Assert.That(result[1], Is.EqualTo("db"));
                Assert.That(result[2], Is.EqualTo("someconstr"));
            });
        }

        [Test]
        public void TestCutSettingsFileArgs_YamlBefore()
        {
            Assert.Multiple(() =>
            {
                Assert.That(Program.CutSettingsFileArgs(new[] { "-y", "myfile.yaml", "review", "db", "someconstr" }, out var result), Is.EqualTo("myfile.yaml"));

                Assert.That(result, Has.Length.EqualTo(3));
                Assert.That(result[0], Is.EqualTo("review"));
                Assert.That(result[1], Is.EqualTo("db"));
                Assert.That(result[2], Is.EqualTo("someconstr"));
            });
        }

        [Test]
        public void TestCutSettingsFileArgs_YamlInMiddle()
        {
            Assert.Multiple(static () =>
            {
                Assert.That(Program.CutSettingsFileArgs(new[] { "review", "-y", "myfile.yaml", "db", "someconstr" }, out var result), Is.EqualTo("myfile.yaml"));

                Assert.That(result, Has.Length.EqualTo(3));
                Assert.That(result[0], Is.EqualTo("review"));
                Assert.That(result[1], Is.EqualTo("db"));
                Assert.That(result[2], Is.EqualTo("someconstr"));
            });
        }

        [Test]
        public void TestDeserialize_SmiServices_DefaultYaml()
        {
            var f = System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory, "default.yaml");
            FileAssert.Exists(f);

            var opts = Program.Deserialize(f, new System.IO.Abstractions.FileSystem());
            Assert.Multiple(() =>
            {
                Assert.That(opts.IsIdentifiableReviewerOptions, Is.Not.Null);
                Assert.That(opts.IsIdentifiableOptions, Is.Not.Null);
            });
        }
    }
}
