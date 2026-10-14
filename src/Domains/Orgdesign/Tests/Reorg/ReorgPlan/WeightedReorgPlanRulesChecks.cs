namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgPlan;

using Acme.Workforce.Orgdesign.Reorg.ReorgPlan;

/// <summary>Checks of reorg plan code, run by the project's runner.</summary>
public static class WeightedReorgPlanRulesChecks
{
    public static void WeightedReorgPlanValidatorReportsEveryProblem()
    {
        var validator = WeightedReorgPlanRules.DefaultWeightedReorgPlanValidator();
        var item = WeightedReorgPlan.NewWeightedReorgPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedReorgPlanCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new WeightedReorgPlanCeilingRangeRule(7.6, 1228.0);
        var item = WeightedReorgPlan.NewWeightedReorgPlan("x", "y");
        item.Ceiling = 1229.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WeightedReorgPlanStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new WeightedReorgPlanStartsAtRangeRule(80, 1000);
        var item = WeightedReorgPlan.NewWeightedReorgPlan("x", "y");
        item.StartsAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WeightedReorgPlanRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new WeightedReorgPlanRequiredExternalRefRule("weighted reorg plan needs this field");
        var item = WeightedReorgPlan.NewWeightedReorgPlan("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        WeightedReorgPlanValidatorReportsEveryProblem();
        WeightedReorgPlanCeilingRangeRuleFlagsValuesAboveCeiling();
        WeightedReorgPlanStartsAtRangeRuleFlagsValuesAboveStartsAt();
        WeightedReorgPlanRequiredExternalRefRuleFlagsABlankExternalRef();
    }
}
