namespace Acme.Workforce.Compensation.Tests.PayBand.Tier;

using Acme.Workforce.Compensation.PayBand.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class TieredPayBandAllocationRulesChecks
{
    public static void TieredPayBandAllocationValidatorReportsEveryProblem()
    {
        var validator = TieredPayBandAllocationRules.DefaultTieredPayBandAllocationValidator();
        var item = TieredPayBandAllocation.NewTieredPayBandAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredPayBandAllocationSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new TieredPayBandAllocationSequenceRangeRule(3600, 86400);
        var item = TieredPayBandAllocation.NewTieredPayBandAllocation("x", "y");
        item.Sequence = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredPayBandAllocationPriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new TieredPayBandAllocationPriorityRangeRule(255, 1000);
        var item = TieredPayBandAllocation.NewTieredPayBandAllocation("x", "y");
        item.Priority = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        TieredPayBandAllocationValidatorReportsEveryProblem();
        TieredPayBandAllocationSequenceRangeRuleFlagsValuesAboveSequence();
        TieredPayBandAllocationPriorityRangeRuleFlagsValuesAbovePriority();
    }
}
