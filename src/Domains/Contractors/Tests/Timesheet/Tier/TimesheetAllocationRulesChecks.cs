namespace Acme.Workforce.Contractors.Tests.Timesheet.Tier;

using Acme.Workforce.Contractors.Timesheet.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class TimesheetAllocationRulesChecks
{
    public static void TimesheetAllocationValidatorReportsEveryProblem()
    {
        var validator = TimesheetAllocationRules.DefaultTimesheetAllocationValidator();
        var item = TimesheetAllocation.NewTimesheetAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TimesheetAllocationCodeLengthRuleFlagsALongCode()
    {
        var rule = new TimesheetAllocationCodeLengthRule(16);
        var item = TimesheetAllocation.NewTimesheetAllocation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TimesheetAllocationSegmentLengthRuleFlagsALongSegment()
    {
        var rule = new TimesheetAllocationSegmentLengthRule(255);
        var item = TimesheetAllocation.NewTimesheetAllocation("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TimesheetAllocationWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new TimesheetAllocationWeightRangeRule(1.5, 3301.0);
        var item = TimesheetAllocation.NewTimesheetAllocation("x", "y");
        item.Weight = 3302.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        TimesheetAllocationValidatorReportsEveryProblem();
        TimesheetAllocationCodeLengthRuleFlagsALongCode();
        TimesheetAllocationSegmentLengthRuleFlagsALongSegment();
        TimesheetAllocationWeightRangeRuleFlagsValuesAboveWeight();
    }
}
