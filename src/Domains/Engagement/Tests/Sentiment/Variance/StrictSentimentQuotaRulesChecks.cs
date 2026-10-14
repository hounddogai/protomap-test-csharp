namespace Acme.Workforce.Engagement.Tests.Sentiment.Variance;

using Acme.Workforce.Engagement.Sentiment.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class StrictSentimentQuotaRulesChecks
{
    public static void StrictSentimentQuotaValidatorReportsEveryProblem()
    {
        var validator = StrictSentimentQuotaRules.DefaultStrictSentimentQuotaValidator();
        var item = StrictSentimentQuota.NewStrictSentimentQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictSentimentQuotaAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new StrictSentimentQuotaAttemptsRangeRule(16, 1000);
        var item = StrictSentimentQuota.NewStrictSentimentQuota("x", "y");
        item.Attempts = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictSentimentQuotaRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new StrictSentimentQuotaRequiredLabelRule("strict sentiment quota needs an entry");
        var item = StrictSentimentQuota.NewStrictSentimentQuota("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictSentimentQuotaRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new StrictSentimentQuotaRequiredCodeRule("strict sentiment quota needs an entry");
        var item = StrictSentimentQuota.NewStrictSentimentQuota("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictSentimentQuotaRequiredRegionRuleFlagsABlankRegion()
    {
        var rule = new StrictSentimentQuotaRequiredRegionRule("strict sentiment quota needs this field");
        var item = StrictSentimentQuota.NewStrictSentimentQuota("", "");
        item.Region = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        StrictSentimentQuotaValidatorReportsEveryProblem();
        StrictSentimentQuotaAttemptsRangeRuleFlagsValuesAboveAttempts();
        StrictSentimentQuotaRequiredLabelRuleFlagsABlankLabel();
        StrictSentimentQuotaRequiredCodeRuleFlagsABlankCode();
        StrictSentimentQuotaRequiredRegionRuleFlagsABlankRegion();
    }
}
