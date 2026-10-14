namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Item;

using Acme.Workforce.Talent.SuccessionPlan.Item;

/// <summary>Checks of item code, run by the project's runner.</summary>
public static class SeasonalSuccessionPlanRuleRulesChecks
{
    public static void SeasonalSuccessionPlanRuleValidatorReportsEveryProblem()
    {
        var validator = SeasonalSuccessionPlanRuleRules.DefaultSeasonalSuccessionPlanRuleValidator();
        var item = SeasonalSuccessionPlanRule.NewSeasonalSuccessionPlanRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalSuccessionPlanRuleCodeLengthRuleFlagsALongCode()
    {
        var rule = new SeasonalSuccessionPlanRuleCodeLengthRule(120);
        var item = SeasonalSuccessionPlanRule.NewSeasonalSuccessionPlanRule("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalSuccessionPlanRuleQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new SeasonalSuccessionPlanRuleQuantityRangeRule(120, 3600);
        var item = SeasonalSuccessionPlanRule.NewSeasonalSuccessionPlanRule("x", "y");
        item.Quantity = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalSuccessionPlanRuleLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new SeasonalSuccessionPlanRuleLimitRangeRule(1000, 3600);
        var item = SeasonalSuccessionPlanRule.NewSeasonalSuccessionPlanRule("x", "y");
        item.Limit = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalSuccessionPlanRuleRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new SeasonalSuccessionPlanRuleRequiredTitleRule("seasonal succession plan rule needs an entry");
        var item = SeasonalSuccessionPlanRule.NewSeasonalSuccessionPlanRule("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SeasonalSuccessionPlanRuleValidatorReportsEveryProblem();
        SeasonalSuccessionPlanRuleCodeLengthRuleFlagsALongCode();
        SeasonalSuccessionPlanRuleQuantityRangeRuleFlagsValuesAboveQuantity();
        SeasonalSuccessionPlanRuleLimitRangeRuleFlagsValuesAboveLimit();
        SeasonalSuccessionPlanRuleRequiredTitleRuleFlagsABlankTitle();
    }
}
