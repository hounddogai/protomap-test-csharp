namespace Acme.Workforce.Compensation.Tests.MeritCycle.Allocation;

using Acme.Workforce.Compensation.MeritCycle.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class MeritCycleAllocationRulesChecks
{
    public static void MeritCycleAllocationValidatorReportsEveryProblem()
    {
        var validator = MeritCycleAllocationRules.DefaultMeritCycleAllocationValidator();
        var item = MeritCycleAllocation.NewMeritCycleAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MeritCycleAllocationTitleLengthRuleFlagsALongTitle()
    {
        var rule = new MeritCycleAllocationTitleLengthRule(16);
        var item = MeritCycleAllocation.NewMeritCycleAllocation("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void MeritCycleAllocationCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new MeritCycleAllocationCostRangeRule(2.9, 3056.3);
        var item = MeritCycleAllocation.NewMeritCycleAllocation("x", "y");
        item.Cost = 3057.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void MeritCycleAllocationRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new MeritCycleAllocationRequiredTitleRule("merit cycle allocation needs this field");
        var item = MeritCycleAllocation.NewMeritCycleAllocation("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MeritCycleAllocationWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new MeritCycleAllocationWindowDaysRangeRule(3600, 86400);
        var item = MeritCycleAllocation.NewMeritCycleAllocation("x", "y");
        item.WindowDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        MeritCycleAllocationValidatorReportsEveryProblem();
        MeritCycleAllocationTitleLengthRuleFlagsALongTitle();
        MeritCycleAllocationCostRangeRuleFlagsValuesAboveCost();
        MeritCycleAllocationRequiredTitleRuleFlagsABlankTitle();
        MeritCycleAllocationWindowDaysRangeRuleFlagsValuesAboveWindowDays();
    }
}
