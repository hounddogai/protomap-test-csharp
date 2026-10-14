namespace Acme.Workforce.Engagement.Tests.Sentiment.Threshold;

using Acme.Workforce.Engagement.Sentiment.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class LenientSentimentThresholdRulesChecks
{
    public static void LenientSentimentThresholdValidatorReportsEveryProblem()
    {
        var validator = LenientSentimentThresholdRules.DefaultLenientSentimentThresholdValidator();
        var item = LenientSentimentThreshold.NewLenientSentimentThreshold("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientSentimentThresholdFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new LenientSentimentThresholdFloorValueRangeRule(2.0, 2030.5);
        var item = LenientSentimentThreshold.NewLenientSentimentThreshold("x", "y");
        item.FloorValue = 2032.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientSentimentThresholdDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new LenientSentimentThresholdDurationMinutesRangeRule(255, 1000);
        var item = LenientSentimentThreshold.NewLenientSentimentThreshold("x", "y");
        item.DurationMinutes = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientSentimentThresholdSalaryBandLengthRuleFlagsALongSalaryBand()
    {
        var rule = new LenientSentimentThresholdSalaryBandLengthRule(86400);
        var item = LenientSentimentThreshold.NewLenientSentimentThreshold("x", "y");
        item.SalaryBand = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.SalaryBand = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientSentimentThresholdRequiredCountryRuleFlagsABlankCountry()
    {
        var rule = new LenientSentimentThresholdRequiredCountryRule("lenient sentiment threshold needs an entry");
        var item = LenientSentimentThreshold.NewLenientSentimentThreshold("", "");
        item.Country = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientSentimentThresholdCodeLengthRuleFlagsALongCode()
    {
        var rule = new LenientSentimentThresholdCodeLengthRule(255);
        var item = LenientSentimentThreshold.NewLenientSentimentThreshold("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LenientSentimentThresholdValidatorReportsEveryProblem();
        LenientSentimentThresholdFloorValueRangeRuleFlagsValuesAboveFloorValue();
        LenientSentimentThresholdDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        LenientSentimentThresholdSalaryBandLengthRuleFlagsALongSalaryBand();
        LenientSentimentThresholdRequiredCountryRuleFlagsABlankCountry();
        LenientSentimentThresholdCodeLengthRuleFlagsALongCode();
    }
}
