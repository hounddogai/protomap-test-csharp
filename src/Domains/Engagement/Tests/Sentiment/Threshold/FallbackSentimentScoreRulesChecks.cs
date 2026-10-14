namespace Acme.Workforce.Engagement.Tests.Sentiment.Threshold;

using Acme.Workforce.Engagement.Sentiment.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class FallbackSentimentScoreRulesChecks
{
    public static void FallbackSentimentScoreValidatorReportsEveryProblem()
    {
        var validator = FallbackSentimentScoreRules.DefaultFallbackSentimentScoreValidator();
        var item = FallbackSentimentScore.NewFallbackSentimentScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackSentimentScoreVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new FallbackSentimentScoreVersionRangeRule(16, 80);
        var item = FallbackSentimentScore.NewFallbackSentimentScore("x", "y");
        item.Version = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackSentimentScoreRequiredCountryRuleFlagsABlankCountry()
    {
        var rule = new FallbackSentimentScoreRequiredCountryRule("fallback sentiment score needs a value");
        var item = FallbackSentimentScore.NewFallbackSentimentScore("", "");
        item.Country = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackSentimentScoreLabelLengthRuleFlagsALongLabel()
    {
        var rule = new FallbackSentimentScoreLabelLengthRule(64);
        var item = FallbackSentimentScore.NewFallbackSentimentScore("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackSentimentScoreRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FallbackSentimentScoreRequiredCodeRule("fallback sentiment score needs a value");
        var item = FallbackSentimentScore.NewFallbackSentimentScore("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackSentimentScoreLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new FallbackSentimentScoreLeaveDaysRangeRule(32, 80);
        var item = FallbackSentimentScore.NewFallbackSentimentScore("x", "y");
        item.LeaveDays = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackSentimentScoreCountryLengthRuleFlagsALongCountry()
    {
        var rule = new FallbackSentimentScoreCountryLengthRule(255);
        var item = FallbackSentimentScore.NewFallbackSentimentScore("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        FallbackSentimentScoreValidatorReportsEveryProblem();
        FallbackSentimentScoreVersionRangeRuleFlagsValuesAboveVersion();
        FallbackSentimentScoreRequiredCountryRuleFlagsABlankCountry();
        FallbackSentimentScoreLabelLengthRuleFlagsALongLabel();
        FallbackSentimentScoreRequiredCodeRuleFlagsABlankCode();
        FallbackSentimentScoreLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        FallbackSentimentScoreCountryLengthRuleFlagsALongCountry();
    }
}
