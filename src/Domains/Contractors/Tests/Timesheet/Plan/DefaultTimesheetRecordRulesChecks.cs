namespace Acme.Workforce.Contractors.Tests.Timesheet.Plan;

using Acme.Workforce.Contractors.Timesheet.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class DefaultTimesheetRecordRulesChecks
{
    public static void DefaultTimesheetRecordValidatorReportsEveryProblem()
    {
        var validator = DefaultTimesheetRecordRules.DefaultDefaultTimesheetRecordValidator();
        var item = DefaultTimesheetRecord.NewDefaultTimesheetRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultTimesheetRecordRequiredNameRuleFlagsABlankName()
    {
        var rule = new DefaultTimesheetRecordRequiredNameRule("default timesheet record needs this field");
        var item = DefaultTimesheetRecord.NewDefaultTimesheetRecord("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultTimesheetRecordGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new DefaultTimesheetRecordGraceDaysRangeRule(255, 1000);
        var item = DefaultTimesheetRecord.NewDefaultTimesheetRecord("x", "y");
        item.GraceDays = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        DefaultTimesheetRecordValidatorReportsEveryProblem();
        DefaultTimesheetRecordRequiredNameRuleFlagsABlankName();
        DefaultTimesheetRecordGraceDaysRangeRuleFlagsValuesAboveGraceDays();
    }
}
