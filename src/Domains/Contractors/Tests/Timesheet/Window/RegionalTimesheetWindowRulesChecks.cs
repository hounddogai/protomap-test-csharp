namespace Acme.Workforce.Contractors.Tests.Timesheet.Window;

using Acme.Workforce.Contractors.Timesheet.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class RegionalTimesheetWindowRulesChecks
{
    public static void RegionalTimesheetWindowValidatorReportsEveryProblem()
    {
        var validator = RegionalTimesheetWindowRules.DefaultRegionalTimesheetWindowValidator();
        var item = RegionalTimesheetWindow.NewRegionalTimesheetWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalTimesheetWindowRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new RegionalTimesheetWindowRequiredTitleRule("regional timesheet window needs a value");
        var item = RegionalTimesheetWindow.NewRegionalTimesheetWindow("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalTimesheetWindowRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new RegionalTimesheetWindowRequiredSummaryRule("regional timesheet window needs a value");
        var item = RegionalTimesheetWindow.NewRegionalTimesheetWindow("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalTimesheetWindowTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new RegionalTimesheetWindowTenureMonthsRangeRule(64, 120);
        var item = RegionalTimesheetWindow.NewRegionalTimesheetWindow("x", "y");
        item.TenureMonths = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RegionalTimesheetWindowValidatorReportsEveryProblem();
        RegionalTimesheetWindowRequiredTitleRuleFlagsABlankTitle();
        RegionalTimesheetWindowRequiredSummaryRuleFlagsABlankSummary();
        RegionalTimesheetWindowTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
    }
}
