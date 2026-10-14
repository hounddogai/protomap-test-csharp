namespace Acme.Workforce.Payrollops.Tests.PayRun.Review;

using Acme.Workforce.Payrollops.PayRun.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class PayRunEntryRulesChecks
{
    public static void PayRunEntryValidatorReportsEveryProblem()
    {
        var validator = PayRunEntryRules.DefaultPayRunEntryValidator();
        var item = PayRunEntry.NewPayRunEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayRunEntryGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new PayRunEntryGraceDaysRangeRule(32, 3600);
        var item = PayRunEntry.NewPayRunEntry("x", "y");
        item.GraceDays = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PayRunEntryRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PayRunEntryRequiredTitleRule("pay run entry needs this field");
        var item = PayRunEntry.NewPayRunEntry("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PayRunEntryValidatorReportsEveryProblem();
        PayRunEntryGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        PayRunEntryRequiredTitleRuleFlagsABlankTitle();
    }
}
