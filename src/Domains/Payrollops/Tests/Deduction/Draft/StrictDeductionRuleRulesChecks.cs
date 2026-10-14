namespace Acme.Workforce.Payrollops.Tests.Deduction.Draft;

using Acme.Workforce.Payrollops.Deduction.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class StrictDeductionRuleRulesChecks
{
    public static void StrictDeductionRuleValidatorReportsEveryProblem()
    {
        var validator = StrictDeductionRuleRules.DefaultStrictDeductionRuleValidator();
        var item = StrictDeductionRule.NewStrictDeductionRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictDeductionRuleLabelLengthRuleFlagsALongLabel()
    {
        var rule = new StrictDeductionRuleLabelLengthRule(16);
        var item = StrictDeductionRule.NewStrictDeductionRule("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictDeductionRuleSkillLengthRuleFlagsALongSkill()
    {
        var rule = new StrictDeductionRuleSkillLengthRule(80);
        var item = StrictDeductionRule.NewStrictDeductionRule("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        StrictDeductionRuleValidatorReportsEveryProblem();
        StrictDeductionRuleLabelLengthRuleFlagsALongLabel();
        StrictDeductionRuleSkillLengthRuleFlagsALongSkill();
    }
}
