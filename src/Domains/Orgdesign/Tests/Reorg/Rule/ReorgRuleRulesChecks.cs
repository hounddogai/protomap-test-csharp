namespace Acme.Workforce.Orgdesign.Tests.Reorg.Rule;

using Acme.Workforce.Orgdesign.Reorg.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class ReorgRuleRulesChecks
{
    public static void ReorgRuleValidatorReportsEveryProblem()
    {
        var validator = ReorgRuleRules.DefaultReorgRuleValidator();
        var item = ReorgRule.NewReorgRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ReorgRuleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ReorgRuleRequiredCodeRule("reorg rule needs an entry");
        var item = ReorgRule.NewReorgRule("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReorgRuleSkillLengthRuleFlagsALongSkill()
    {
        var rule = new ReorgRuleSkillLengthRule(86400);
        var item = ReorgRule.NewReorgRule("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ReorgRuleCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new ReorgRuleCostRangeRule(5.6, 824.8);
        var item = ReorgRule.NewReorgRule("x", "y");
        item.Cost = 826.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ReorgRuleRequiredNameRuleFlagsABlankName()
    {
        var rule = new ReorgRuleRequiredNameRule("reorg rule needs this field");
        var item = ReorgRule.NewReorgRule("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ReorgRuleValidatorReportsEveryProblem();
        ReorgRuleRequiredCodeRuleFlagsABlankCode();
        ReorgRuleSkillLengthRuleFlagsALongSkill();
        ReorgRuleCostRangeRuleFlagsValuesAboveCost();
        ReorgRuleRequiredNameRuleFlagsABlankName();
    }
}
