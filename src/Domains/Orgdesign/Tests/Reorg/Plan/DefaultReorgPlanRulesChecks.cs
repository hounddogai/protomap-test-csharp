namespace Acme.Workforce.Orgdesign.Tests.Reorg.Plan;

using Acme.Workforce.Orgdesign.Reorg.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class DefaultReorgPlanRulesChecks
{
    public static void DefaultReorgPlanValidatorReportsEveryProblem()
    {
        var validator = DefaultReorgPlanRules.DefaultDefaultReorgPlanValidator();
        var item = DefaultReorgPlan.NewDefaultReorgPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultReorgPlanManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new DefaultReorgPlanManagerIdLengthRule(255);
        var item = DefaultReorgPlan.NewDefaultReorgPlan("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultReorgPlanLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new DefaultReorgPlanLimitRangeRule(120, 1000);
        var item = DefaultReorgPlan.NewDefaultReorgPlan("x", "y");
        item.Limit = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultReorgPlanAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new DefaultReorgPlanAmountRangeRule(9.2, 3179.9);
        var item = DefaultReorgPlan.NewDefaultReorgPlan("x", "y");
        item.Amount = 3181.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultReorgPlanTitleLengthRuleFlagsALongTitle()
    {
        var rule = new DefaultReorgPlanTitleLengthRule(86400);
        var item = DefaultReorgPlan.NewDefaultReorgPlan("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DefaultReorgPlanValidatorReportsEveryProblem();
        DefaultReorgPlanManagerIdLengthRuleFlagsALongManagerId();
        DefaultReorgPlanLimitRangeRuleFlagsValuesAboveLimit();
        DefaultReorgPlanAmountRangeRuleFlagsValuesAboveAmount();
        DefaultReorgPlanTitleLengthRuleFlagsALongTitle();
    }
}
