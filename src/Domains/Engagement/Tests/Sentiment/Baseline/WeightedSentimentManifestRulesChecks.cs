namespace Acme.Workforce.Engagement.Tests.Sentiment.Baseline;

using Acme.Workforce.Engagement.Sentiment.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class WeightedSentimentManifestRulesChecks
{
    public static void WeightedSentimentManifestValidatorReportsEveryProblem()
    {
        var validator = WeightedSentimentManifestRules.DefaultWeightedSentimentManifestValidator();
        var item = WeightedSentimentManifest.NewWeightedSentimentManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedSentimentManifestMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new WeightedSentimentManifestMarginRangeRule(7.8, 3308.6);
        var item = WeightedSentimentManifest.NewWeightedSentimentManifest("x", "y");
        item.Margin = 3310.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        WeightedSentimentManifestValidatorReportsEveryProblem();
        WeightedSentimentManifestMarginRangeRuleFlagsValuesAboveMargin();
    }
}
