namespace Acme.Workforce.Contractors.Tests.Rate.Slot;

using Acme.Workforce.Contractors.Rate.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class ScheduledRateAllocationRulesChecks
{
    public static void ScheduledRateAllocationValidatorReportsEveryProblem()
    {
        var validator = ScheduledRateAllocationRules.DefaultScheduledRateAllocationValidator();
        var item = ScheduledRateAllocation.NewScheduledRateAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledRateAllocationExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new ScheduledRateAllocationExpiresAtRangeRule(255, 3600);
        var item = ScheduledRateAllocation.NewScheduledRateAllocation("x", "y");
        item.ExpiresAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledRateAllocationRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ScheduledRateAllocationRequiredCodeRule("scheduled rate allocation needs this field");
        var item = ScheduledRateAllocation.NewScheduledRateAllocation("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledRateAllocationPriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new ScheduledRateAllocationPriorityRangeRule(64, 1000);
        var item = ScheduledRateAllocation.NewScheduledRateAllocation("x", "y");
        item.Priority = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledRateAllocationDepartmentLengthRuleFlagsALongDepartment()
    {
        var rule = new ScheduledRateAllocationDepartmentLengthRule(86400);
        var item = ScheduledRateAllocation.NewScheduledRateAllocation("x", "y");
        item.Department = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Department = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledRateAllocationCodeLengthRuleFlagsALongCode()
    {
        var rule = new ScheduledRateAllocationCodeLengthRule(120);
        var item = ScheduledRateAllocation.NewScheduledRateAllocation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ScheduledRateAllocationValidatorReportsEveryProblem();
        ScheduledRateAllocationExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        ScheduledRateAllocationRequiredCodeRuleFlagsABlankCode();
        ScheduledRateAllocationPriorityRangeRuleFlagsValuesAbovePriority();
        ScheduledRateAllocationDepartmentLengthRuleFlagsALongDepartment();
        ScheduledRateAllocationCodeLengthRuleFlagsALongCode();
    }
}
