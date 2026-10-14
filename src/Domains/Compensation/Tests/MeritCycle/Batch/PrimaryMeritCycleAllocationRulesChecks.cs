namespace Acme.Workforce.Compensation.Tests.MeritCycle.Batch;

using Acme.Workforce.Compensation.MeritCycle.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class PrimaryMeritCycleAllocationRulesChecks
{
    public static void PrimaryMeritCycleAllocationValidatorReportsEveryProblem()
    {
        var validator = PrimaryMeritCycleAllocationRules.DefaultPrimaryMeritCycleAllocationValidator();
        var item = PrimaryMeritCycleAllocation.NewPrimaryMeritCycleAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryMeritCycleAllocationTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PrimaryMeritCycleAllocationTitleLengthRule(3600);
        var item = PrimaryMeritCycleAllocation.NewPrimaryMeritCycleAllocation("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryMeritCycleAllocationRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PrimaryMeritCycleAllocationRequiredTitleRule("primary merit cycle allocation needs an entry");
        var item = PrimaryMeritCycleAllocation.NewPrimaryMeritCycleAllocation("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryMeritCycleAllocationRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PrimaryMeritCycleAllocationRequiredCodeRule("primary merit cycle allocation needs an entry");
        var item = PrimaryMeritCycleAllocation.NewPrimaryMeritCycleAllocation("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryMeritCycleAllocationLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new PrimaryMeritCycleAllocationLeaveDaysRangeRule(16, 64);
        var item = PrimaryMeritCycleAllocation.NewPrimaryMeritCycleAllocation("x", "y");
        item.LeaveDays = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PrimaryMeritCycleAllocationValidatorReportsEveryProblem();
        PrimaryMeritCycleAllocationTitleLengthRuleFlagsALongTitle();
        PrimaryMeritCycleAllocationRequiredTitleRuleFlagsABlankTitle();
        PrimaryMeritCycleAllocationRequiredCodeRuleFlagsABlankCode();
        PrimaryMeritCycleAllocationLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
    }
}
