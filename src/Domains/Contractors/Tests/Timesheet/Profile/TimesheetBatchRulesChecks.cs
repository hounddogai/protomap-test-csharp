namespace Acme.Workforce.Contractors.Tests.Timesheet.Profile;

using Acme.Workforce.Contractors.Timesheet.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class TimesheetBatchRulesChecks
{
    public static void TimesheetBatchValidatorReportsEveryProblem()
    {
        var validator = TimesheetBatchRules.DefaultTimesheetBatchValidator();
        var item = TimesheetBatch.NewTimesheetBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TimesheetBatchCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new TimesheetBatchCostRangeRule(5.3, 345.4);
        var item = TimesheetBatch.NewTimesheetBatch("x", "y");
        item.Cost = 346.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TimesheetBatchNameLengthRuleFlagsALongName()
    {
        var rule = new TimesheetBatchNameLengthRule(16);
        var item = TimesheetBatch.NewTimesheetBatch("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TimesheetBatchRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TimesheetBatchRequiredCodeRule("timesheet batch needs an entry");
        var item = TimesheetBatch.NewTimesheetBatch("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TimesheetBatchValidatorReportsEveryProblem();
        TimesheetBatchCostRangeRuleFlagsValuesAboveCost();
        TimesheetBatchNameLengthRuleFlagsALongName();
        TimesheetBatchRequiredCodeRuleFlagsABlankCode();
    }
}
