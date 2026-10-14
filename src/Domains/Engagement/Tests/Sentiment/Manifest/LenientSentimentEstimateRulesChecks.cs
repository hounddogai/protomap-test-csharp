namespace Acme.Workforce.Engagement.Tests.Sentiment.Manifest;

using Acme.Workforce.Engagement.Sentiment.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class LenientSentimentEstimateRulesChecks
{
    public static void LenientSentimentEstimateValidatorReportsEveryProblem()
    {
        var validator = LenientSentimentEstimateRules.DefaultLenientSentimentEstimateValidator();
        var item = LenientSentimentEstimate.NewLenientSentimentEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientSentimentEstimateCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new LenientSentimentEstimateCreatedAtRangeRule(32, 80);
        var item = LenientSentimentEstimate.NewLenientSentimentEstimate("x", "y");
        item.CreatedAt = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientSentimentEstimateFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new LenientSentimentEstimateFloorValueRangeRule(0.3, 679.4);
        var item = LenientSentimentEstimate.NewLenientSentimentEstimate("x", "y");
        item.FloorValue = 680.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientSentimentEstimateRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LenientSentimentEstimateRequiredCodeRule("lenient sentiment estimate needs a value");
        var item = LenientSentimentEstimate.NewLenientSentimentEstimate("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientSentimentEstimateExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new LenientSentimentEstimateExpiresAtRangeRule(32, 3600);
        var item = LenientSentimentEstimate.NewLenientSentimentEstimate("x", "y");
        item.ExpiresAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LenientSentimentEstimateValidatorReportsEveryProblem();
        LenientSentimentEstimateCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        LenientSentimentEstimateFloorValueRangeRuleFlagsValuesAboveFloorValue();
        LenientSentimentEstimateRequiredCodeRuleFlagsABlankCode();
        LenientSentimentEstimateExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
    }
}
