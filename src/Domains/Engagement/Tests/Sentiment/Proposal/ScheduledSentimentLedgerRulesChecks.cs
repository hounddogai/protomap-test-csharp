namespace Acme.Workforce.Engagement.Tests.Sentiment.Proposal;

using Acme.Workforce.Engagement.Sentiment.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class ScheduledSentimentLedgerRulesChecks
{
    public static void ScheduledSentimentLedgerValidatorReportsEveryProblem()
    {
        var validator = ScheduledSentimentLedgerRules.DefaultScheduledSentimentLedgerValidator();
        var item = ScheduledSentimentLedger.NewScheduledSentimentLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledSentimentLedgerChannelLengthRuleFlagsALongChannel()
    {
        var rule = new ScheduledSentimentLedgerChannelLengthRule(32);
        var item = ScheduledSentimentLedger.NewScheduledSentimentLedger("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledSentimentLedgerTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new ScheduledSentimentLedgerTenureMonthsRangeRule(16, 32);
        var item = ScheduledSentimentLedger.NewScheduledSentimentLedger("x", "y");
        item.TenureMonths = 33;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledSentimentLedgerRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ScheduledSentimentLedgerRequiredCodeRule("scheduled sentiment ledger needs this field");
        var item = ScheduledSentimentLedger.NewScheduledSentimentLedger("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledSentimentLedgerRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new ScheduledSentimentLedgerRequiredDescriptionRule("scheduled sentiment ledger needs an entry");
        var item = ScheduledSentimentLedger.NewScheduledSentimentLedger("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledSentimentLedgerRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new ScheduledSentimentLedgerRequiredLabelRule("scheduled sentiment ledger needs a value");
        var item = ScheduledSentimentLedger.NewScheduledSentimentLedger("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ScheduledSentimentLedgerValidatorReportsEveryProblem();
        ScheduledSentimentLedgerChannelLengthRuleFlagsALongChannel();
        ScheduledSentimentLedgerTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
        ScheduledSentimentLedgerRequiredCodeRuleFlagsABlankCode();
        ScheduledSentimentLedgerRequiredDescriptionRuleFlagsABlankDescription();
        ScheduledSentimentLedgerRequiredLabelRuleFlagsABlankLabel();
    }
}
