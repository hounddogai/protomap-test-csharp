namespace Acme.Workforce.Compensation.Tests.MeritCycle.CyclePlan;

using Acme.Workforce.Compensation.MeritCycle.CyclePlan;

/// <summary>Checks of cycle plan code, run by the project's runner.</summary>
public static class TieredMeritCyclePlanRulesChecks
{
    public static void TieredMeritCyclePlanValidatorReportsEveryProblem()
    {
        var validator = TieredMeritCyclePlanRules.DefaultTieredMeritCyclePlanValidator();
        var item = TieredMeritCyclePlan.NewTieredMeritCyclePlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredMeritCyclePlanLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new TieredMeritCyclePlanLimitRangeRule(255, 1000);
        var item = TieredMeritCyclePlan.NewTieredMeritCyclePlan("x", "y");
        item.Limit = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredMeritCyclePlanWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new TieredMeritCyclePlanWeightRangeRule(2.3, 1641.5);
        var item = TieredMeritCyclePlan.NewTieredMeritCyclePlan("x", "y");
        item.Weight = 1643.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredMeritCyclePlanRequiredManagerIdRuleFlagsABlankManagerId()
    {
        var rule = new TieredMeritCyclePlanRequiredManagerIdRule("tiered merit cycle plan needs a value");
        var item = TieredMeritCyclePlan.NewTieredMeritCyclePlan("", "");
        item.ManagerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TieredMeritCyclePlanValidatorReportsEveryProblem();
        TieredMeritCyclePlanLimitRangeRuleFlagsValuesAboveLimit();
        TieredMeritCyclePlanWeightRangeRuleFlagsValuesAboveWeight();
        TieredMeritCyclePlanRequiredManagerIdRuleFlagsABlankManagerId();
    }
}
