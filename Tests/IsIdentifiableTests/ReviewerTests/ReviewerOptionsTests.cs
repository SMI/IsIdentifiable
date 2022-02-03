using IsIdentifiableReviewer;
using NUnit.Framework;

namespace IsIdentifiableTests.ReviewerTests
{
    class ReviewerOptionsTests
    {
        [Test]
        public void TestFillMissingWithValuesUsing_MissingValues()
        {
            var global =  new IsIdentifiableReviewerOptions();
            var local = new IsIdentifiableReviewerOptions();

            global.IgnoreList = "aa";
            global.RedList = "bb";
            global.TargetsFile = "cc";
            global.Theme = "dd";

            local.InheritValuesFrom(global);

            Assert.AreEqual("aa", local.IgnoreList);
            Assert.AreEqual("bb", local.RedList);
            Assert.AreEqual("cc", local.TargetsFile);
            Assert.AreEqual("dd", local.Theme);
        }
        [Test]
        public void TestFillMissingWithValuesUsing_DoNotOverride()
        {
            var global = new IsIdentifiableReviewerOptions();
            var local = new IsIdentifiableReviewerOptions();

            global.IgnoreList = "aa";
            global.RedList = "bb";
            global.TargetsFile = "cc";
            global.Theme = "dd";


            local.IgnoreList = "11";
            local.RedList = "22";
            local.TargetsFile = "33";
            local.Theme = "44";

            local.InheritValuesFrom(global);

            Assert.AreEqual("11", local.IgnoreList);
            Assert.AreEqual("22", local.RedList);
            Assert.AreEqual("33", local.TargetsFile);
            Assert.AreEqual("44", local.Theme);
        }
    }
}
