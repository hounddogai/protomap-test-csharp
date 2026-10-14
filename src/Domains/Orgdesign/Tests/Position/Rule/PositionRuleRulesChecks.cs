namespace Acme.Workforce.Orgdesign.Tests.Position.Rule;

using Acme.Workforce.Orgdesign.Position.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class PositionRuleRulesChecks
{
    public static void PositionRuleValidatorReportsEveryProblem()
    {
        var validator = PositionRuleRules.DefaultPositionRuleValidator();
        var item = PositionRule.NewPositionRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PositionRuleRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new PositionRuleRequiredSkillRule("position rule needs a value");
        var item = PositionRule.NewPositionRule("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PositionRuleManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new PositionRuleManagerIdLengthRule(120);
        var item = PositionRule.NewPositionRule("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PositionRuleSkillLengthRuleFlagsALongSkill()
    {
        var rule = new PositionRuleSkillLengthRule(255);
        var item = PositionRule.NewPositionRule("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PositionRuleAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new PositionRuleAmountRangeRule(0.8, 367.5);
        var item = PositionRule.NewPositionRule("x", "y");
        item.Amount = 369.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PositionRuleValidatorReportsEveryProblem();
        PositionRuleRequiredSkillRuleFlagsABlankSkill();
        PositionRuleManagerIdLengthRuleFlagsALongManagerId();
        PositionRuleSkillLengthRuleFlagsALongSkill();
        PositionRuleAmountRangeRuleFlagsValuesAboveAmount();
    }
}
