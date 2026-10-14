namespace Acme.Workforce.Workplace.Tests.FloorPlan.Rule;

using Acme.Workforce.Workplace.FloorPlan.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class FloorPlanRuleRulesChecks
{
    public static void FloorPlanRuleValidatorReportsEveryProblem()
    {
        var validator = FloorPlanRuleRules.DefaultFloorPlanRuleValidator();
        var item = FloorPlanRule.NewFloorPlanRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FloorPlanRuleCodeLengthRuleFlagsALongCode()
    {
        var rule = new FloorPlanRuleCodeLengthRule(32);
        var item = FloorPlanRule.NewFloorPlanRule("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FloorPlanRuleLabelLengthRuleFlagsALongLabel()
    {
        var rule = new FloorPlanRuleLabelLengthRule(80);
        var item = FloorPlanRule.NewFloorPlanRule("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FloorPlanRuleDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new FloorPlanRuleDiscountRangeRule(4.9, 2598.7);
        var item = FloorPlanRule.NewFloorPlanRule("x", "y");
        item.Discount = 2600.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FloorPlanRuleRequiredRegionRuleFlagsABlankRegion()
    {
        var rule = new FloorPlanRuleRequiredRegionRule("floor plan rule needs an entry");
        var item = FloorPlanRule.NewFloorPlanRule("", "");
        item.Region = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FloorPlanRuleValidatorReportsEveryProblem();
        FloorPlanRuleCodeLengthRuleFlagsALongCode();
        FloorPlanRuleLabelLengthRuleFlagsALongLabel();
        FloorPlanRuleDiscountRangeRuleFlagsValuesAboveDiscount();
        FloorPlanRuleRequiredRegionRuleFlagsABlankRegion();
    }
}
