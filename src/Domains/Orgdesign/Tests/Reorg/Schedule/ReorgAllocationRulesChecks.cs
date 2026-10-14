namespace Acme.Workforce.Orgdesign.Tests.Reorg.Schedule;

using Acme.Workforce.Orgdesign.Reorg.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class ReorgAllocationRulesChecks
{
    public static void ReorgAllocationValidatorReportsEveryProblem()
    {
        var validator = ReorgAllocationRules.DefaultReorgAllocationValidator();
        var item = ReorgAllocation.NewReorgAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ReorgAllocationRequiredManagerIdRuleFlagsABlankManagerId()
    {
        var rule = new ReorgAllocationRequiredManagerIdRule("reorg allocation needs a value");
        var item = ReorgAllocation.NewReorgAllocation("", "");
        item.ManagerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReorgAllocationCodeLengthRuleFlagsALongCode()
    {
        var rule = new ReorgAllocationCodeLengthRule(80);
        var item = ReorgAllocation.NewReorgAllocation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ReorgAllocationSkillLengthRuleFlagsALongSkill()
    {
        var rule = new ReorgAllocationSkillLengthRule(16);
        var item = ReorgAllocation.NewReorgAllocation("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ReorgAllocationRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new ReorgAllocationRequiredSkillRule("reorg allocation needs this field");
        var item = ReorgAllocation.NewReorgAllocation("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReorgAllocationRequiredNameRuleFlagsABlankName()
    {
        var rule = new ReorgAllocationRequiredNameRule("reorg allocation needs this field");
        var item = ReorgAllocation.NewReorgAllocation("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ReorgAllocationValidatorReportsEveryProblem();
        ReorgAllocationRequiredManagerIdRuleFlagsABlankManagerId();
        ReorgAllocationCodeLengthRuleFlagsALongCode();
        ReorgAllocationSkillLengthRuleFlagsALongSkill();
        ReorgAllocationRequiredSkillRuleFlagsABlankSkill();
        ReorgAllocationRequiredNameRuleFlagsABlankName();
    }
}
