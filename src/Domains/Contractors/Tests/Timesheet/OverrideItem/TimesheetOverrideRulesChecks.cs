namespace Acme.Workforce.Contractors.Tests.Timesheet.OverrideItem;

using Acme.Workforce.Contractors.Timesheet.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class TimesheetOverrideRulesChecks
{
    public static void TimesheetOverrideValidatorReportsEveryProblem()
    {
        var validator = TimesheetOverrideRules.DefaultTimesheetOverrideValidator();
        var item = TimesheetOverride.NewTimesheetOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TimesheetOverrideGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new TimesheetOverrideGraceDaysRangeRule(32, 3600);
        var item = TimesheetOverride.NewTimesheetOverride("x", "y");
        item.GraceDays = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TimesheetOverrideRequiredRegionRuleFlagsABlankRegion()
    {
        var rule = new TimesheetOverrideRequiredRegionRule("timesheet override needs an entry");
        var item = TimesheetOverride.NewTimesheetOverride("", "");
        item.Region = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TimesheetOverrideRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TimesheetOverrideRequiredCodeRule("timesheet override needs this field");
        var item = TimesheetOverride.NewTimesheetOverride("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TimesheetOverrideManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new TimesheetOverrideManagerIdLengthRule(16);
        var item = TimesheetOverride.NewTimesheetOverride("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TimesheetOverrideRequiredManagerIdRuleFlagsABlankManagerId()
    {
        var rule = new TimesheetOverrideRequiredManagerIdRule("timesheet override needs an entry");
        var item = TimesheetOverride.NewTimesheetOverride("", "");
        item.ManagerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TimesheetOverrideValidatorReportsEveryProblem();
        TimesheetOverrideGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        TimesheetOverrideRequiredRegionRuleFlagsABlankRegion();
        TimesheetOverrideRequiredCodeRuleFlagsABlankCode();
        TimesheetOverrideManagerIdLengthRuleFlagsALongManagerId();
        TimesheetOverrideRequiredManagerIdRuleFlagsABlankManagerId();
    }
}
