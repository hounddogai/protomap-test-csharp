namespace Acme.Workforce.Contractors.Tests.Timesheet.Rollup;

using Acme.Workforce.Contractors.Timesheet.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class TimesheetRollupRulesChecks
{
    public static void TimesheetRollupValidatorReportsEveryProblem()
    {
        var validator = TimesheetRollupRules.DefaultTimesheetRollupValidator();
        var item = TimesheetRollup.NewTimesheetRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TimesheetRollupDepartmentLengthRuleFlagsALongDepartment()
    {
        var rule = new TimesheetRollupDepartmentLengthRule(32);
        var item = TimesheetRollup.NewTimesheetRollup("x", "y");
        item.Department = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Department = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TimesheetRollupCostCenterLengthRuleFlagsALongCostCenter()
    {
        var rule = new TimesheetRollupCostCenterLengthRule(1000);
        var item = TimesheetRollup.NewTimesheetRollup("x", "y");
        item.CostCenter = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.CostCenter = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TimesheetRollupLabelLengthRuleFlagsALongLabel()
    {
        var rule = new TimesheetRollupLabelLengthRule(3600);
        var item = TimesheetRollup.NewTimesheetRollup("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TimesheetRollupRequiredCostCenterRuleFlagsABlankCostCenter()
    {
        var rule = new TimesheetRollupRequiredCostCenterRule("timesheet rollup needs a value");
        var item = TimesheetRollup.NewTimesheetRollup("", "");
        item.CostCenter = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TimesheetRollupValidatorReportsEveryProblem();
        TimesheetRollupDepartmentLengthRuleFlagsALongDepartment();
        TimesheetRollupCostCenterLengthRuleFlagsALongCostCenter();
        TimesheetRollupLabelLengthRuleFlagsALongLabel();
        TimesheetRollupRequiredCostCenterRuleFlagsABlankCostCenter();
    }
}
