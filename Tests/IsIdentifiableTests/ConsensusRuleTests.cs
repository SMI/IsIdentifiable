using IsIdentifiable.Failures;
using IsIdentifiable.Rules;
using IsIdentifiable.Runners;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace IsIdentifiable.Tests;

class ConsensusRuleTests
{
    [Test]
    public void NoConsensus_OnlyOneHasProblem()
    {
        var rule = new ConsensusRule()
        {
            Rules = new IAppliableRule[]
            {
                new TestRule(RuleAction.Report,new FailurePart("bb",FailureClassification.Person)),
                new TestRule(RuleAction.Ignore),
            }
        };

        var result = rule.Apply("ff", "vv", out var badParts);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(RuleAction.None));
            Assert.That(badParts, Is.Empty);
        });
    }

    [TestCase(-1)]
    [TestCase(10)]
    public void Consensus_Exact(int offset)
    {
        var rule = new ConsensusRule()
        {
            Rules = new IAppliableRule[]
            {
                new TestRule(RuleAction.Report,new FailurePart("bb",FailureClassification.Person,offset)),
                new TestRule(RuleAction.Report,new FailurePart("bb",FailureClassification.Person,offset)),
            }
        };

        var result = rule.Apply("ff", "vv", out var badParts);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(RuleAction.Report));
            Assert.That(badParts.Single().Offset, Is.EqualTo(offset));
        });
    }

    [Test]
    public void Consensus_SingleOverlap()
    {
        var rule = new ConsensusRule()
        {
            Rules = new IAppliableRule[]
            {
                // Word is length 2 and begins at offset 10
                new TestRule(RuleAction.Report,new FailurePart("ab",FailureClassification.Person,10)),
                new TestRule(RuleAction.Report,new FailurePart("bc",FailureClassification.Person,11)),
            }
        };

        var result = rule.Apply("ff", "abc is so cool", out var badParts);

        Assert.That(result, Is.EqualTo(RuleAction.Report));
        var badPart = badParts.Single();
        Assert.Multiple(() =>
        {
            Assert.That(badPart.Offset, Is.EqualTo(10));
            Assert.That(badPart.Word, Is.EqualTo("ab"));
        });
    }

    [Test]
    public void TestDeserialization()
    {
        var yaml =
            @"ConsensusRules:
    - Rules:
      - !SocketRule
          Host: 127.0.123.123
          Port: 1234
      - !SocketRule
          Host: 127.0.123.123
          Port: 567";


        var deserializer = IsIdentifiableAbstractRunner.GetDeserializer();
        var ruleSet = deserializer.Deserialize<RuleSet>(yaml);

        Assert.That(ruleSet.ConsensusRules.Single(), Is.InstanceOf(typeof(ConsensusRule)));
        Assert.Multiple(() =>
        {
            Assert.That(ruleSet.ConsensusRules.Single().Rules[0], Is.InstanceOf(typeof(SocketRule)));
            Assert.That(((SocketRule)ruleSet.ConsensusRules.Single().Rules[0]).Port, Is.EqualTo(1234));
            Assert.That(((SocketRule)ruleSet.ConsensusRules.Single().Rules[1]).Port, Is.EqualTo(567));
        });
    }

    internal class TestRule : IAppliableRule
    {
        private readonly RuleAction _rule;
        private readonly FailurePart[] _parts;

        public TestRule(RuleAction rule, params FailurePart[] parts)
        {
            _parts = parts;
            _rule = rule;
        }

        public RuleAction Apply(string fieldName, string fieldValue, out List<FailurePart> badParts)
        {
            badParts = _parts.ToList();

            return _rule;
        }
    }
}
