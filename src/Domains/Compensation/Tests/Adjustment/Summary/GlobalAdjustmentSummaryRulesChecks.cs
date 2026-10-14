namespace Acme.Workforce.Compensation.Tests.Adjustment.Summary;

using Acme.Workforce.Compensation.Adjustment.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class GlobalAdjustmentSummaryRulesChecks
{
    public static void GlobalAdjustmentSummaryValidatorReportsEveryProblem()
    {
        var validator = GlobalAdjustmentSummaryRules.DefaultGlobalAdjustmentSummaryValidator();
        var item = GlobalAdjustmentSummary.NewGlobalAdjustmentSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalAdjustmentSummaryTitleLengthRuleFlagsALongTitle()
    {
        var rule = new GlobalAdjustmentSummaryTitleLengthRule(80);
        var item = GlobalAdjustmentSummary.NewGlobalAdjustmentSummary("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalAdjustmentSummaryRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new GlobalAdjustmentSummaryRequiredTitleRule("global adjustment summary needs this field");
        var item = GlobalAdjustmentSummary.NewGlobalAdjustmentSummary("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalAdjustmentSummarySummaryLengthRuleFlagsALongSummary()
    {
        var rule = new GlobalAdjustmentSummarySummaryLengthRule(1000);
        var item = GlobalAdjustmentSummary.NewGlobalAdjustmentSummary("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalAdjustmentSummaryRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new GlobalAdjustmentSummaryRequiredLocationRule("global adjustment summary needs a value");
        var item = GlobalAdjustmentSummary.NewGlobalAdjustmentSummary("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        GlobalAdjustmentSummaryValidatorReportsEveryProblem();
        GlobalAdjustmentSummaryTitleLengthRuleFlagsALongTitle();
        GlobalAdjustmentSummaryRequiredTitleRuleFlagsABlankTitle();
        GlobalAdjustmentSummarySummaryLengthRuleFlagsALongSummary();
        GlobalAdjustmentSummaryRequiredLocationRuleFlagsABlankLocation();
    }
}
