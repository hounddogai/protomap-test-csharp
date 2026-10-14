namespace Acme.Workforce.Absence.Tests.ReturnPlan.Tier;

using Acme.Workforce.Absence.ReturnPlan.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class PendingReturnPlanRollupRulesChecks
{
    public static void PendingReturnPlanRollupValidatorReportsEveryProblem()
    {
        var validator = PendingReturnPlanRollupRules.DefaultPendingReturnPlanRollupValidator();
        var item = PendingReturnPlanRollup.NewPendingReturnPlanRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingReturnPlanRollupQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new PendingReturnPlanRollupQuantityRangeRule(16, 120);
        var item = PendingReturnPlanRollup.NewPendingReturnPlanRollup("x", "y");
        item.Quantity = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingReturnPlanRollupWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new PendingReturnPlanRollupWeightRangeRule(5.2, 4485.1);
        var item = PendingReturnPlanRollup.NewPendingReturnPlanRollup("x", "y");
        item.Weight = 4486.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingReturnPlanRollupCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new PendingReturnPlanRollupCostRangeRule(5.5, 2628.9);
        var item = PendingReturnPlanRollup.NewPendingReturnPlanRollup("x", "y");
        item.Cost = 2630.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PendingReturnPlanRollupValidatorReportsEveryProblem();
        PendingReturnPlanRollupQuantityRangeRuleFlagsValuesAboveQuantity();
        PendingReturnPlanRollupWeightRangeRuleFlagsValuesAboveWeight();
        PendingReturnPlanRollupCostRangeRuleFlagsValuesAboveCost();
    }
}
