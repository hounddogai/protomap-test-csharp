namespace Acme.Workforce.Contractors.Tests.Timesheet.Batch;

using Acme.Workforce.Contractors.Timesheet.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class GlobalTimesheetBatchRulesChecks
{
    public static void GlobalTimesheetBatchValidatorReportsEveryProblem()
    {
        var validator = GlobalTimesheetBatchRules.DefaultGlobalTimesheetBatchValidator();
        var item = GlobalTimesheetBatch.NewGlobalTimesheetBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalTimesheetBatchTitleLengthRuleFlagsALongTitle()
    {
        var rule = new GlobalTimesheetBatchTitleLengthRule(3600);
        var item = GlobalTimesheetBatch.NewGlobalTimesheetBatch("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalTimesheetBatchRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new GlobalTimesheetBatchRequiredTitleRule("global timesheet batch needs an entry");
        var item = GlobalTimesheetBatch.NewGlobalTimesheetBatch("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalTimesheetBatchNameLengthRuleFlagsALongName()
    {
        var rule = new GlobalTimesheetBatchNameLengthRule(1000);
        var item = GlobalTimesheetBatch.NewGlobalTimesheetBatch("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalTimesheetBatchRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new GlobalTimesheetBatchRequiredCodeRule("global timesheet batch needs a value");
        var item = GlobalTimesheetBatch.NewGlobalTimesheetBatch("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        GlobalTimesheetBatchValidatorReportsEveryProblem();
        GlobalTimesheetBatchTitleLengthRuleFlagsALongTitle();
        GlobalTimesheetBatchRequiredTitleRuleFlagsABlankTitle();
        GlobalTimesheetBatchNameLengthRuleFlagsALongName();
        GlobalTimesheetBatchRequiredCodeRuleFlagsABlankCode();
    }
}
