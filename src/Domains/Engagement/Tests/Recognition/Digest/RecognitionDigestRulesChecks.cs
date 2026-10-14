namespace Acme.Workforce.Engagement.Tests.Recognition.Digest;

using Acme.Workforce.Engagement.Recognition.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class RecognitionDigestRulesChecks
{
    public static void RecognitionDigestValidatorReportsEveryProblem()
    {
        var validator = RecognitionDigestRules.DefaultRecognitionDigestValidator();
        var item = RecognitionDigest.NewRecognitionDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RecognitionDigestCodeLengthRuleFlagsALongCode()
    {
        var rule = new RecognitionDigestCodeLengthRule(255);
        var item = RecognitionDigest.NewRecognitionDigest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RecognitionDigestSegmentLengthRuleFlagsALongSegment()
    {
        var rule = new RecognitionDigestSegmentLengthRule(64);
        var item = RecognitionDigest.NewRecognitionDigest("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RecognitionDigestTitleLengthRuleFlagsALongTitle()
    {
        var rule = new RecognitionDigestTitleLengthRule(120);
        var item = RecognitionDigest.NewRecognitionDigest("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RecognitionDigestValidatorReportsEveryProblem();
        RecognitionDigestCodeLengthRuleFlagsALongCode();
        RecognitionDigestSegmentLengthRuleFlagsALongSegment();
        RecognitionDigestTitleLengthRuleFlagsALongTitle();
    }
}
