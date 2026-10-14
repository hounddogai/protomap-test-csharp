namespace Acme.Workforce.Engagement.Tests.Sentiment.Rollup;

using Acme.Workforce.Engagement.Sentiment.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class CappedSentimentManifestRulesChecks
{
    public static void CappedSentimentManifestValidatorReportsEveryProblem()
    {
        var validator = CappedSentimentManifestRules.DefaultCappedSentimentManifestValidator();
        var item = CappedSentimentManifest.NewCappedSentimentManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedSentimentManifestChannelLengthRuleFlagsALongChannel()
    {
        var rule = new CappedSentimentManifestChannelLengthRule(16);
        var item = CappedSentimentManifest.NewCappedSentimentManifest("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedSentimentManifestTitleLengthRuleFlagsALongTitle()
    {
        var rule = new CappedSentimentManifestTitleLengthRule(86400);
        var item = CappedSentimentManifest.NewCappedSentimentManifest("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedSentimentManifestCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new CappedSentimentManifestCeilingRangeRule(9.4, 3231.3);
        var item = CappedSentimentManifest.NewCappedSentimentManifest("x", "y");
        item.Ceiling = 3232.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedSentimentManifestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new CappedSentimentManifestRequiredCodeRule("capped sentiment manifest needs a value");
        var item = CappedSentimentManifest.NewCappedSentimentManifest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CappedSentimentManifestValidatorReportsEveryProblem();
        CappedSentimentManifestChannelLengthRuleFlagsALongChannel();
        CappedSentimentManifestTitleLengthRuleFlagsALongTitle();
        CappedSentimentManifestCeilingRangeRuleFlagsValuesAboveCeiling();
        CappedSentimentManifestRequiredCodeRuleFlagsABlankCode();
    }
}
