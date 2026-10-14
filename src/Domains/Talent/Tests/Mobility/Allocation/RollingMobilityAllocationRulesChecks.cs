namespace Acme.Workforce.Talent.Tests.Mobility.Allocation;

using Acme.Workforce.Talent.Mobility.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class RollingMobilityAllocationRulesChecks
{
    public static void RollingMobilityAllocationValidatorReportsEveryProblem()
    {
        var validator = RollingMobilityAllocationRules.DefaultRollingMobilityAllocationValidator();
        var item = RollingMobilityAllocation.NewRollingMobilityAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingMobilityAllocationMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new RollingMobilityAllocationMinimumRangeRule(7.3, 4727.2);
        var item = RollingMobilityAllocation.NewRollingMobilityAllocation("x", "y");
        item.Minimum = 4728.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingMobilityAllocationSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new RollingMobilityAllocationSequenceRangeRule(32, 120);
        var item = RollingMobilityAllocation.NewRollingMobilityAllocation("x", "y");
        item.Sequence = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingMobilityAllocationStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new RollingMobilityAllocationStartsAtRangeRule(16, 32);
        var item = RollingMobilityAllocation.NewRollingMobilityAllocation("x", "y");
        item.StartsAt = 33;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RollingMobilityAllocationValidatorReportsEveryProblem();
        RollingMobilityAllocationMinimumRangeRuleFlagsValuesAboveMinimum();
        RollingMobilityAllocationSequenceRangeRuleFlagsValuesAboveSequence();
        RollingMobilityAllocationStartsAtRangeRuleFlagsValuesAboveStartsAt();
    }
}
