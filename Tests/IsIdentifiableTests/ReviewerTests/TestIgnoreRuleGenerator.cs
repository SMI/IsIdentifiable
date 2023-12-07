using IsIdentifiable.Failures;
using IsIdentifiable.Redacting;
using NUnit.Framework;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;

namespace IsIdentifiable.Tests.ReviewerTests;

class TestIgnoreRuleGenerator
{
    private MockFileSystem _fileSystem;

    [SetUp]
    public void SetUp()
    {
        _fileSystem = new MockFileSystem();
    }

    [Test]
    public void TestRepeatedIgnoring()
    {
        var failure = new Failure(
            new FailurePart[]
            {
                new("Kansas", FailureClassification.Location, 13),
                new("Toto", FailureClassification.Location, 28)
            })
        {
            ProblemValue = "We aren't in Kansas anymore Toto",
            ProblemField = "Narrative",
            ResourcePrimaryKey = "1.2.3.4"
        };

        var newRules = _fileSystem.FileInfo.New("IgnoreList.yaml");

        var ignorer = new IgnoreRuleGenerator(_fileSystem, newRules);

        //it should be novel i.e. require user decision
        Assert.That(ignorer.OnLoad(failure, out _), Is.True);

        //we tell it to ignore this value
        ignorer.Add(failure);

        TestHelpers.Contains(
            @"- Action: Ignore
  IfColumn: Narrative
  IfPattern: ^We\ aren't\ in\ Kansas\ anymore\ Toto$
", _fileSystem.File.ReadAllText(newRules.FullName)); //btw slash space is a 'literal space' so legit

        //it should be no longer be novel
        Assert.That(ignorer.OnLoad(failure, out _), Is.False);

    }


    [Test]
    public void Test_SaveYaml()
    {
        var failure1 = new Failure(
            new FailurePart[]
            {
                new("Hadock", FailureClassification.Location, 0),
            })
        {
            ProblemValue = "Hadock",
            ProblemField = "Narrative",
            ResourcePrimaryKey = "1.2.3.4"
        };
        var failure2 = new Failure(
            new FailurePart[]
            {
                new("Bass", FailureClassification.Location, 0),
            })
        {
            ProblemValue = "Bass",
            ProblemField = "Narrative",
            ResourcePrimaryKey = "1.2.3.5"
        };

        var newRules = _fileSystem.FileInfo.New("IgnoreList.yaml");

        var ignorer = new IgnoreRuleGenerator(_fileSystem, newRules);


        //we tell it to ignore this value
        ignorer.Add(failure1);
        ignorer.Add(failure2);

        TestHelpers.Contains(
            @"
- Action: Ignore
  IfColumn: Narrative
  IfPattern: ^Hadock$".Trim(), _fileSystem.File.ReadAllText(newRules.FullName));

        TestHelpers.Contains(
            @"
- Action: Ignore
  IfColumn: Narrative
  IfPattern: ^Bass$".Trim(), _fileSystem.File.ReadAllText(newRules.FullName));

        ignorer.Rules.Remove(ignorer.Rules.Last());
        ignorer.Save();


        TestHelpers.Contains(
            @"
- Action: Ignore
  IfColumn: Narrative
  IfPattern: ^Hadock$".Trim(), _fileSystem.File.ReadAllText(newRules.FullName));

        TestHelpers.DoesNotContain(
            @"
- Action: Ignore
  IfColumn: Narrative
  IfPattern: ^Bass$".Trim(), _fileSystem.File.ReadAllText(newRules.FullName));


        ignorer.Rules.Clear();
        ignorer.Save();
        Assert.That(string.IsNullOrWhiteSpace(_fileSystem.File.ReadAllText(newRules.FullName)), Is.True);


    }

    [Test]
    public void TestUndo()
    {
        var failure = new Failure(
            new FailurePart[]
            {
                new("Kansas", FailureClassification.Location, 13),
                new("Toto", FailureClassification.Location, 28)
            })
        {
            ProblemValue = "We aren't in Kansas anymore Toto",
            ProblemField = "Narrative",
            ResourcePrimaryKey = "1.2.3.4"
        };

        var newRules = _fileSystem.FileInfo.New("IgnoreList.yaml");

        //create an existing rule to check that Undo doesn't just nuke the entire file
        _fileSystem.File.WriteAllText(newRules.FullName, @"- Action: Ignore
  IfColumn: Narrative
  IfPattern: ^Joker Wuz Ere$
");

        var ignorer = new IgnoreRuleGenerator(_fileSystem, newRules);

        //it should be novel i.e. require user decision
        Assert.That(ignorer.OnLoad(failure, out _), Is.True);

        //we tell it to ignore this value
        ignorer.Add(failure);

        TestHelpers.Contains(
            @"- Action: Ignore
  IfColumn: Narrative
  IfPattern: ^We\ aren't\ in\ Kansas\ anymore\ Toto$
", _fileSystem.File.ReadAllText(newRules.FullName)); //btw slash space is a 'literal space' so legit

        Assert.Multiple(() =>
        {
            //it should be no longer be novel
            Assert.That(ignorer.OnLoad(failure, out _), Is.False);

            //Undo
            Assert.That(ignorer.History, Has.Count.EqualTo(1));
            Assert.That(ignorer.Rules, Has.Count.EqualTo(2));
        });
        ignorer.Undo();

        Assert.Multiple(() =>
        {
            Assert.That(ignorer.History, Is.Empty);
            Assert.That(ignorer.Rules, Has.Count.EqualTo(1));

            //only the original one should be there
            Assert.That(_fileSystem.File.ReadAllText(newRules.FullName), Is.EqualTo(@"- Action: Ignore
  IfColumn: Narrative
  IfPattern: ^Joker Wuz Ere$
"));
        });

        //repeated undo calls do nothing
        ignorer.Undo();
        ignorer.Undo();
        ignorer.Undo();
    }


    [Test]
    public void Test_DeleteRule()
    {
        var failure = new Failure(
            new FailurePart[]
            {
                new("Kansas", FailureClassification.Location, 13),
                new("Toto", FailureClassification.Location, 28)
            })
        {
            ProblemValue = "We aren't in Kansas anymore Toto",
            ProblemField = "Narrative",
            ResourcePrimaryKey = "1.2.3.4"
        };

        var newRules = _fileSystem.FileInfo.New("IgnoreList.yaml");

        //create an existing rule to check that Undo doesn't just nuke the entire file
        _fileSystem.File.WriteAllText(newRules.FullName, @"- Action: Ignore
  IfColumn: Narrative
  IfPattern: ^Joker Wuz Ere$
");

        var ignorer = new IgnoreRuleGenerator(_fileSystem, newRules);

        //it should be novel i.e. require user decision
        Assert.That(ignorer.OnLoad(failure, out _), Is.True);

        //we tell it to ignore this value
        ignorer.Add(failure);

        TestHelpers.Contains(
            @"- Action: Ignore
  IfColumn: Narrative
  IfPattern: ^We\ aren't\ in\ Kansas\ anymore\ Toto$
", _fileSystem.File.ReadAllText(newRules.FullName)); //btw slash space is a 'literal space' so legit

        Assert.Multiple(() =>
        {
            //it should be no longer be novel
            Assert.That(ignorer.OnLoad(failure, out _), Is.False);

            //Remove the last one
            Assert.That(ignorer.Rules, Has.Count.EqualTo(2));
        });
        var result = ignorer.Delete(ignorer.Rules[1]);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);

            //deleted from memory
            Assert.That(ignorer.Rules, Has.Count.EqualTo(1));
        });


        var newRulebaseYaml = _fileSystem.File.ReadAllText(newRules.FullName);

        //only the original one should be there
        Assert.That(newRulebaseYaml, Does.Contain(@"- Action: Ignore
  IfColumn: Narrative
  IfPattern: ^Joker Wuz Ere$
"));

        Assert.That(newRulebaseYaml, Does.Contain("# Rule deleted by "));

        Assert.That(newRulebaseYaml, Does.Not.Contain("Kansas"));

        //repeated undo calls do nothing
        ignorer.Undo();
        ignorer.Undo();
        ignorer.Undo();
    }
}
