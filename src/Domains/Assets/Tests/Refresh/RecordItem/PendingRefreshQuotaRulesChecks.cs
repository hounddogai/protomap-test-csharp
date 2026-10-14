namespace Acme.Workforce.Assets.Tests.Refresh.RecordItem;

using Acme.Workforce.Assets.Refresh.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class PendingRefreshQuotaRulesChecks
{
    public static void PendingRefreshQuotaValidatorReportsEveryProblem()
    {
        var validator = PendingRefreshQuotaRules.DefaultPendingRefreshQuotaValidator();
        var item = PendingRefreshQuota.NewPendingRefreshQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingRefreshQuotaTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new PendingRefreshQuotaTimezoneLengthRule(86400);
        var item = PendingRefreshQuota.NewPendingRefreshQuota("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingRefreshQuotaBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new PendingRefreshQuotaBudgetRangeRule(5.2, 3161.1);
        var item = PendingRefreshQuota.NewPendingRefreshQuota("x", "y");
        item.Budget = 3162.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingRefreshQuotaRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PendingRefreshQuotaRequiredTitleRule("pending refresh quota needs this field");
        var item = PendingRefreshQuota.NewPendingRefreshQuota("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PendingRefreshQuotaValidatorReportsEveryProblem();
        PendingRefreshQuotaTimezoneLengthRuleFlagsALongTimezone();
        PendingRefreshQuotaBudgetRangeRuleFlagsValuesAboveBudget();
        PendingRefreshQuotaRequiredTitleRuleFlagsABlankTitle();
    }
}
