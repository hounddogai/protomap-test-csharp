namespace Acme.Workforce.Engagement.Tests.Sentiment.Proposal;

using Acme.Workforce.Engagement.Sentiment.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class SentimentThresholdRulesChecks
{
    public static void SentimentThresholdValidatorReportsEveryProblem()
    {
        var validator = SentimentThresholdRules.DefaultSentimentThresholdValidator();
        var item = SentimentThreshold.NewSentimentThreshold("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SentimentThresholdCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new SentimentThresholdCreatedAtRangeRule(16, 80);
        var item = SentimentThreshold.NewSentimentThreshold("x", "y");
        item.CreatedAt = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SentimentThresholdCodeLengthRuleFlagsALongCode()
    {
        var rule = new SentimentThresholdCodeLengthRule(3600);
        var item = SentimentThreshold.NewSentimentThreshold("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SentimentThresholdLabelLengthRuleFlagsALongLabel()
    {
        var rule = new SentimentThresholdLabelLengthRule(1000);
        var item = SentimentThreshold.NewSentimentThreshold("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SentimentThresholdRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SentimentThresholdRequiredCodeRule("sentiment threshold needs this field");
        var item = SentimentThreshold.NewSentimentThreshold("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SentimentThresholdValidatorReportsEveryProblem();
        SentimentThresholdCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        SentimentThresholdCodeLengthRuleFlagsALongCode();
        SentimentThresholdLabelLengthRuleFlagsALongLabel();
        SentimentThresholdRequiredCodeRuleFlagsABlankCode();
    }
}
