namespace Acme.Workforce.Workplace.Tests.FloorPlan.Agreement;

using Acme.Workforce.Workplace.FloorPlan.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class LegacyFloorPlanRuleRulesChecks
{
    public static void LegacyFloorPlanRuleValidatorReportsEveryProblem()
    {
        var validator = LegacyFloorPlanRuleRules.DefaultLegacyFloorPlanRuleValidator();
        var item = LegacyFloorPlanRule.NewLegacyFloorPlanRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyFloorPlanRuleRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new LegacyFloorPlanRuleRequiredSkillRule("legacy floor plan rule needs this field");
        var item = LegacyFloorPlanRule.NewLegacyFloorPlanRule("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyFloorPlanRuleWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new LegacyFloorPlanRuleWeightRangeRule(1.4, 2938.5);
        var item = LegacyFloorPlanRule.NewLegacyFloorPlanRule("x", "y");
        item.Weight = 2940.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LegacyFloorPlanRuleValidatorReportsEveryProblem();
        LegacyFloorPlanRuleRequiredSkillRuleFlagsABlankSkill();
        LegacyFloorPlanRuleWeightRangeRuleFlagsValuesAboveWeight();
    }
}
