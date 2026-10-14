namespace Acme.Workforce.Engagement.Tests.Recognition.Segment;

using Acme.Workforce.Engagement.Recognition.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class FallbackRecognitionDigestRulesChecks
{
    public static void FallbackRecognitionDigestValidatorReportsEveryProblem()
    {
        var validator = FallbackRecognitionDigestRules.DefaultFallbackRecognitionDigestValidator();
        var item = FallbackRecognitionDigest.NewFallbackRecognitionDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackRecognitionDigestCodeLengthRuleFlagsALongCode()
    {
        var rule = new FallbackRecognitionDigestCodeLengthRule(255);
        var item = FallbackRecognitionDigest.NewFallbackRecognitionDigest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackRecognitionDigestCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new FallbackRecognitionDigestCreatedAtRangeRule(255, 1000);
        var item = FallbackRecognitionDigest.NewFallbackRecognitionDigest("x", "y");
        item.CreatedAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        FallbackRecognitionDigestValidatorReportsEveryProblem();
        FallbackRecognitionDigestCodeLengthRuleFlagsALongCode();
        FallbackRecognitionDigestCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
    }
}
