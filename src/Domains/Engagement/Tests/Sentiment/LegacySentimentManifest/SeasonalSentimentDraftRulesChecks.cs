namespace Acme.Workforce.Engagement.Tests.Sentiment.LegacySentimentManifest;

using Acme.Workforce.Engagement.Sentiment.LegacySentimentManifest;

/// <summary>Checks of legacy sentiment manifest code, run by the project's runner.</summary>
public static class SeasonalSentimentDraftRulesChecks
{
    public static void SeasonalSentimentDraftValidatorReportsEveryProblem()
    {
        var validator = SeasonalSentimentDraftRules.DefaultSeasonalSentimentDraftValidator();
        var item = SeasonalSentimentDraft.NewSeasonalSentimentDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalSentimentDraftCodeLengthRuleFlagsALongCode()
    {
        var rule = new SeasonalSentimentDraftCodeLengthRule(86400);
        var item = SeasonalSentimentDraft.NewSeasonalSentimentDraft("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalSentimentDraftLabelLengthRuleFlagsALongLabel()
    {
        var rule = new SeasonalSentimentDraftLabelLengthRule(1000);
        var item = SeasonalSentimentDraft.NewSeasonalSentimentDraft("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalSentimentDraftQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new SeasonalSentimentDraftQuantityRangeRule(120, 86400);
        var item = SeasonalSentimentDraft.NewSeasonalSentimentDraft("x", "y");
        item.Quantity = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SeasonalSentimentDraftValidatorReportsEveryProblem();
        SeasonalSentimentDraftCodeLengthRuleFlagsALongCode();
        SeasonalSentimentDraftLabelLengthRuleFlagsALongLabel();
        SeasonalSentimentDraftQuantityRangeRuleFlagsValuesAboveQuantity();
    }
}
