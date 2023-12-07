using IsIdentifiable.Failures;
using IsIdentifiable.Rules;
using NUnit.Framework;
using System;
using System.Linq;

namespace IsIdentifiable.Tests;

class SocketRuleTests
{
    [Test]
    public void TestSocket_NegativeResponse()
    {
        var bad = SocketRule.HandleResponse("\0");
        Assert.That(bad, Is.Empty);
    }

    [Test]
    public void TestSocket_PositiveResponse()
    {
        var bad = SocketRule.HandleResponse("Person\010\0Dave\0").Single();

        Assert.Multiple(() =>
        {
            Assert.That(bad.Classification, Is.EqualTo(FailureClassification.Person));
            Assert.That(bad.Offset, Is.EqualTo(10));
            Assert.That(bad.Word, Is.EqualTo("Dave"));
        });
    }

    [Test]
    public void TestSocket_TwoPositiveResponses()
    {
        var bad = SocketRule.HandleResponse("Person\010\0Dave\0ORGANIZATION\00\0The University of Dundee\0").ToArray();

        Assert.That(bad, Has.Length.EqualTo(2));

        Assert.Multiple(() =>
        {
            Assert.That(bad[0].Classification, Is.EqualTo(FailureClassification.Person));
            Assert.That(bad[0].Offset, Is.EqualTo(10));
            Assert.That(bad[0].Word, Is.EqualTo("Dave"));

            Assert.That(bad[1].Classification, Is.EqualTo(FailureClassification.Organization));
            Assert.That(bad[1].Offset, Is.EqualTo(0));
            Assert.That(bad[1].Word, Is.EqualTo("The University of Dundee"));
        });
    }

    [Test]
    public void TestSocket_InvalidResponses()
    {
        var ex = Assert.Throws<Exception>(() => SocketRule.HandleResponse("Cadbury\010\0Cream Egg\0").ToArray());
        Assert.That(ex?.Message, Does.Contain("'Cadbury' (expected a member of Enum FailureClassification)"));

        ex = Assert.Throws<Exception>(() => SocketRule.HandleResponse("Person\0fish\0Cream Egg\0").ToArray());
        Assert.That(ex?.Message, Does.Contain("Response was 'fish' (expected int)"));

        ex = Assert.Throws<Exception>(() => SocketRule.HandleResponse("Person\0").ToArray());
        Assert.That(ex?.Message, Does.Contain("Expected tokens to arrive in multiples of 3 (but got '1')"));
    }
}
