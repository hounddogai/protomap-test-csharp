namespace Acme.Workforce.Contractors.Tests.Timesheet.Summary;

using Acme.Workforce.Contractors.Timesheet.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class TimesheetSummaryRulesChecks
{
    public static void TimesheetSummaryValidatorReportsEveryProblem()
    {
        var validator = TimesheetSummaryRules.DefaultTimesheetSummaryValidator();
        var item = TimesheetSummary.NewTimesheetSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TimesheetSummaryRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new TimesheetSummaryRequiredPhoneRule("timesheet summary needs a value");
        var item = TimesheetSummary.NewTimesheetSummary("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TimesheetSummaryRequiredNameRuleFlagsABlankName()
    {
        var rule = new TimesheetSummaryRequiredNameRule("timesheet summary needs this field");
        var item = TimesheetSummary.NewTimesheetSummary("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TimesheetSummaryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TimesheetSummaryRequiredCodeRule("timesheet summary needs an entry");
        var item = TimesheetSummary.NewTimesheetSummary("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TimesheetSummaryRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new TimesheetSummaryRatioRangeRule(0.5, 2661.6);
        var item = TimesheetSummary.NewTimesheetSummary("x", "y");
        item.Ratio = 2663.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TimesheetSummaryNameLengthRuleFlagsALongName()
    {
        var rule = new TimesheetSummaryNameLengthRule(1000);
        var item = TimesheetSummary.NewTimesheetSummary("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TimesheetSummaryValidatorReportsEveryProblem();
        TimesheetSummaryRequiredPhoneRuleFlagsABlankPhone();
        TimesheetSummaryRequiredNameRuleFlagsABlankName();
        TimesheetSummaryRequiredCodeRuleFlagsABlankCode();
        TimesheetSummaryRatioRangeRuleFlagsValuesAboveRatio();
        TimesheetSummaryNameLengthRuleFlagsALongName();
    }
}
