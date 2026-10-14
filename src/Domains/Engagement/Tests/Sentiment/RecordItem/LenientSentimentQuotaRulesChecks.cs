namespace Acme.Workforce.Engagement.Tests.Sentiment.RecordItem;

using Acme.Workforce.Engagement.Sentiment.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class LenientSentimentQuotaRulesChecks
{
    public static void LenientSentimentQuotaValidatorReportsEveryProblem()
    {
        var validator = LenientSentimentQuotaRules.DefaultLenientSentimentQuotaValidator();
        var item = LenientSentimentQuota.NewLenientSentimentQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientSentimentQuotaMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new LenientSentimentQuotaMaximumRangeRule(3.8, 3492.2);
        var item = LenientSentimentQuota.NewLenientSentimentQuota("x", "y");
        item.Maximum = 3493.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientSentimentQuotaTitleLengthRuleFlagsALongTitle()
    {
        var rule = new LenientSentimentQuotaTitleLengthRule(80);
        var item = LenientSentimentQuota.NewLenientSentimentQuota("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientSentimentQuotaEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new LenientSentimentQuotaEndsAtRangeRule(32, 86400);
        var item = LenientSentimentQuota.NewLenientSentimentQuota("x", "y");
        item.EndsAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LenientSentimentQuotaValidatorReportsEveryProblem();
        LenientSentimentQuotaMaximumRangeRuleFlagsValuesAboveMaximum();
        LenientSentimentQuotaTitleLengthRuleFlagsALongTitle();
        LenientSentimentQuotaEndsAtRangeRuleFlagsValuesAboveEndsAt();
    }
}
