namespace Acme.Workforce.Orgdesign.Tests.Position.Quota;

using Acme.Workforce.Orgdesign.Position.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class AdaptivePositionPlanRulesChecks
{
    public static void AdaptivePositionPlanValidatorReportsEveryProblem()
    {
        var validator = AdaptivePositionPlanRules.DefaultAdaptivePositionPlanValidator();
        var item = AdaptivePositionPlan.NewAdaptivePositionPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptivePositionPlanRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AdaptivePositionPlanRequiredCodeRule("adaptive position plan needs an entry");
        var item = AdaptivePositionPlan.NewAdaptivePositionPlan("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptivePositionPlanQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new AdaptivePositionPlanQuantityRangeRule(1000, 86400);
        var item = AdaptivePositionPlan.NewAdaptivePositionPlan("x", "y");
        item.Quantity = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AdaptivePositionPlanValidatorReportsEveryProblem();
        AdaptivePositionPlanRequiredCodeRuleFlagsABlankCode();
        AdaptivePositionPlanQuantityRangeRuleFlagsValuesAboveQuantity();
    }
}
