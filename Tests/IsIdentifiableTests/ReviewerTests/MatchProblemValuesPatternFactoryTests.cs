﻿using IsIdentifiable.Failures;
using IsIdentifiable.Redacting;
using NUnit.Framework;

namespace IsIdentifiable.Tests.ReviewerTests;

public class MatchProblemValuesPatternFactoryTests
{
    [Test]
    public void OverlappingMatches_SinglePart()
    {
        var f = new Failure(new[]
            {
                new FailurePart("F",FailureClassification.Person,0),
            })
        { ProblemValue = "Frequent Problems" };

        var factory = new MatchProblemValuesPatternFactory();
        Assert.That(factory.GetPattern(null, f), Is.EqualTo("^(F)"));
    }

    [Test]
    public void OverlappingMatches_ExactOverlap()
    {
        var f = new Failure(new[]
            {
                new FailurePart("Freq",FailureClassification.Person,0),
                new FailurePart("Freq",FailureClassification.Organization,0),
            })
        { ProblemValue = "Frequent Problems" };

        var factory = new MatchProblemValuesPatternFactory();
        Assert.That(factory.GetPattern(null, f), Is.EqualTo("^(Freq)"));
    }
    [Test]
    public void OverlappingMatches_OffsetOverlaps()
    {
        var f = new Failure(new[]
            {
                new FailurePart("req",FailureClassification.Person,1),
                new FailurePart("q",FailureClassification.Organization,3),
            })
        { ProblemValue = "Frequent Problems" };

        var factory = new MatchProblemValuesPatternFactory();

        //fallback onto full match because of overlapping problem words
        Assert.That(factory.GetPattern(null, f), Is.EqualTo("(req)"));
    }

    [Test]
    public void OverlappingMatches_NoOverlaps()
    {
        var f = new Failure(new[]
            {
                new FailurePart("re",FailureClassification.Person,1),
                new FailurePart("quent",FailureClassification.Organization,3),
            })
        { ProblemValue = "Frequent Problems" };

        var factory = new MatchProblemValuesPatternFactory();
        Assert.That(factory.GetPattern(null, f), Is.EqualTo("(requent)"));
    }
}
