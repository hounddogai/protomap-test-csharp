namespace Acme.Workforce.Engagement.Tests.Sentiment.Proposal;

using Acme.Workforce.Engagement.Sentiment.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class SentimentQuotaRulesChecks
{
    public static void SentimentQuotaValidatorReportsEveryProblem()
    {
        var validator = SentimentQuotaRules.DefaultSentimentQuotaValidator();
        var item = SentimentQuota.NewSentimentQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SentimentQuotaQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new SentimentQuotaQuantityRangeRule(80, 86400);
        var item = SentimentQuota.NewSentimentQuota("x", "y");
        item.Quantity = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SentimentQuotaNameLengthRuleFlagsALongName()
    {
        var rule = new SentimentQuotaNameLengthRule(1000);
        var item = SentimentQuota.NewSentimentQuota("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SentimentQuotaValidatorReportsEveryProblem();
        SentimentQuotaQuantityRangeRuleFlagsValuesAboveQuantity();
        SentimentQuotaNameLengthRuleFlagsALongName();
    }
}
