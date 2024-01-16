using IsIdentifiable.Options;
using NUnit.Framework;

namespace IsIdentifiable.Tests.ReviewerTests;

class ReviewerOptionsTests
{
    [Test]
    public void TestFillMissingWithValuesUsing_MissingValues()
    {
        var global = new IsIdentifiableReviewerOptions();
        var local = new IsIdentifiableReviewerOptions();

        global.IgnoreList = "aa";
        global.Reportlist = "bb";
        global.TargetsFile = "cc";
        global.Theme = "dd";

        local.InheritValuesFrom(global);

        Assert.Multiple(() =>
        {
            Assert.That(local.IgnoreList, Is.EqualTo("aa"));
            Assert.That(local.Reportlist, Is.EqualTo("bb"));
            Assert.That(local.TargetsFile, Is.EqualTo("cc"));
            Assert.That(local.Theme, Is.EqualTo("dd"));
        });
    }
    [Test]
    public void TestFillMissingWithValuesUsing_DoNotOverride()
    {
        var global = new IsIdentifiableReviewerOptions();
        var local = new IsIdentifiableReviewerOptions();

        global.IgnoreList = "aa";
        global.Reportlist = "bb";
        global.TargetsFile = "cc";
        global.Theme = "dd";


        local.IgnoreList = "11";
        local.Reportlist = "22";
        local.TargetsFile = "33";
        local.Theme = "44";

        local.InheritValuesFrom(global);

        Assert.Multiple(() =>
        {
            Assert.That(local.IgnoreList, Is.EqualTo("11"));
            Assert.That(local.Reportlist, Is.EqualTo("22"));
            Assert.That(local.TargetsFile, Is.EqualTo("33"));
            Assert.That(local.Theme, Is.EqualTo("44"));
        });
    }
}
