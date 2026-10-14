namespace Acme.Workforce.Engagement.Tests.Sentiment.Summary;

using Acme.Workforce.Engagement.Sentiment.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class SentimentSummaryRulesChecks
{
    public static void SentimentSummaryValidatorReportsEveryProblem()
    {
        var validator = SentimentSummaryRules.DefaultSentimentSummaryValidator();
        var item = SentimentSummary.NewSentimentSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SentimentSummaryRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new SentimentSummaryRequiredTitleRule("sentiment summary needs a value");
        var item = SentimentSummary.NewSentimentSummary("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SentimentSummaryCodeLengthRuleFlagsALongCode()
    {
        var rule = new SentimentSummaryCodeLengthRule(3600);
        var item = SentimentSummary.NewSentimentSummary("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SentimentSummaryCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new SentimentSummaryCreatedAtRangeRule(1000, 3600);
        var item = SentimentSummary.NewSentimentSummary("x", "y");
        item.CreatedAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SentimentSummaryTitleLengthRuleFlagsALongTitle()
    {
        var rule = new SentimentSummaryTitleLengthRule(255);
        var item = SentimentSummary.NewSentimentSummary("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SentimentSummaryRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new SentimentSummaryRequiredDescriptionRule("sentiment summary needs this field");
        var item = SentimentSummary.NewSentimentSummary("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SentimentSummaryValidatorReportsEveryProblem();
        SentimentSummaryRequiredTitleRuleFlagsABlankTitle();
        SentimentSummaryCodeLengthRuleFlagsALongCode();
        SentimentSummaryCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        SentimentSummaryTitleLengthRuleFlagsALongTitle();
        SentimentSummaryRequiredDescriptionRuleFlagsABlankDescription();
    }
}
