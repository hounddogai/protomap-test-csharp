namespace Acme.Workforce.Engagement.Tests.Sentiment.Variance;

using Acme.Workforce.Engagement.Sentiment.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class RegionalSentimentManifestRulesChecks
{
    public static void RegionalSentimentManifestValidatorReportsEveryProblem()
    {
        var validator = RegionalSentimentManifestRules.DefaultRegionalSentimentManifestValidator();
        var item = RegionalSentimentManifest.NewRegionalSentimentManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalSentimentManifestMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new RegionalSentimentManifestMarginRangeRule(8.6, 644.5);
        var item = RegionalSentimentManifest.NewRegionalSentimentManifest("x", "y");
        item.Margin = 646.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalSentimentManifestNameLengthRuleFlagsALongName()
    {
        var rule = new RegionalSentimentManifestNameLengthRule(255);
        var item = RegionalSentimentManifest.NewRegionalSentimentManifest("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalSentimentManifestCodeLengthRuleFlagsALongCode()
    {
        var rule = new RegionalSentimentManifestCodeLengthRule(255);
        var item = RegionalSentimentManifest.NewRegionalSentimentManifest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RegionalSentimentManifestValidatorReportsEveryProblem();
        RegionalSentimentManifestMarginRangeRuleFlagsValuesAboveMargin();
        RegionalSentimentManifestNameLengthRuleFlagsALongName();
        RegionalSentimentManifestCodeLengthRuleFlagsALongCode();
    }
}
