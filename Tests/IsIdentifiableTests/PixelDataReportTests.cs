using IsIdentifiable.Failures;
using IsIdentifiable.Redacting;
using IsIdentifiable.Reporting;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace IsIdentifiable.Tests
{
    internal class PixelDataReportTests
    {
        [Test]
        public void TestReportReader_PixelReport()
        {
            var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "pixeldatareport.csv");

            var reader = new ReportReader(new FileInfo(path), (s) => { }, CancellationToken.None);
            Assert.IsNotEmpty(reader.Failures);
        }


        [Test]
        public void MatchProblemValuesPatternFactory_PixelReport()
        {
            var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "pixeldatareport.csv");

            var reader = new ReportReader(new FileInfo(path), (s) => { }, CancellationToken.None);

            for (int i = 0; i < reader.Failures.Length; i++)
            {
                Failure l = reader.Failures[i];
                new MatchProblemValuesPatternFactory().GetPattern(this, l);
            }
        }

        [Test]
        public void IgnoreRuleGenerator_PixelReport()
        {
            var f = GetPixelFailure(out var ocrOutput);
            var g = new IgnoreRuleGenerator();
            Assert.AreEqual($"^{Regex.Escape(ocrOutput)}$",g.RulesFactory.GetPattern(this,f),"When the user ignores OCR data the ignore pattern should exactly match the full text discovered");
        }

        [Test]
        public void UpdateRuleGenerator_PixelReport()
        {
            var f = GetPixelFailure(out var ocrOutput);
            var g = new RowUpdater();
            Assert.AreEqual($"^{Regex.Escape(ocrOutput)}$", g.RulesFactory.GetPattern(this, f), "When the user markes problematic the OCR data the pattern should exactly match the full text discovered");
        }

        private Failure GetPixelFailure(out string ocrOutput)
        {
            ocrOutput = @"12  w ! i  e  16  17  it  19  P B \ B K e N 10 iR 0 ) f  LB 1 1 1 1 LB 1 1 G w 3 In 1     =2  P  4  22 i  P     P        INRINET] INRINET] IARINEN] INRINET] INRINET] INRINET] INRINET] INRINEN]  028 PLe 30 e e 33 2y 35 1] O L @ L @ L G L 6 1] @ L @ L  INRINET] INRINET] INRINET] INRINET] INRINET] INRINEN] ITNINET     @  Bl  Y4  o  it        m  e  Gx     45  2";

            return new Failure(new[] { new FailurePart(ocrOutput, FailureClassification.PixelText, -1) })
            {
                Resource = "/home/thomas/testdicoms2/gdcmConformanceTests/CT_OSIRIX_OddOverlay.dcm",
                ResourcePrimaryKey = "1.3.12.2.1107.5.1.4.51771.30050005122714151602300000095",
                ProblemField = "PixelData",
                ProblemValue = ocrOutput
            };
        }
    }
}
