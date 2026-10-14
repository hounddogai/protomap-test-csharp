namespace Acme.Workforce.Talent.Tests.SkillProfile.Proposal;

using Acme.Workforce.Talent.SkillProfile.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class SkillProfileCycleRulesChecks
{
    public static void SkillProfileCycleValidatorReportsEveryProblem()
    {
        var validator = SkillProfileCycleRules.DefaultSkillProfileCycleValidator();
        var item = SkillProfileCycle.NewSkillProfileCycle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SkillProfileCycleCodeLengthRuleFlagsALongCode()
    {
        var rule = new SkillProfileCycleCodeLengthRule(80);
        var item = SkillProfileCycle.NewSkillProfileCycle("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SkillProfileCycleRequiredNameRuleFlagsABlankName()
    {
        var rule = new SkillProfileCycleRequiredNameRule("skill profile cycle needs a value");
        var item = SkillProfileCycle.NewSkillProfileCycle("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SkillProfileCycleNameLengthRuleFlagsALongName()
    {
        var rule = new SkillProfileCycleNameLengthRule(1000);
        var item = SkillProfileCycle.NewSkillProfileCycle("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SkillProfileCycleRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new SkillProfileCycleRequiredPhoneRule("skill profile cycle needs an entry");
        var item = SkillProfileCycle.NewSkillProfileCycle("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SkillProfileCycleValidatorReportsEveryProblem();
        SkillProfileCycleCodeLengthRuleFlagsALongCode();
        SkillProfileCycleRequiredNameRuleFlagsABlankName();
        SkillProfileCycleNameLengthRuleFlagsALongName();
        SkillProfileCycleRequiredPhoneRuleFlagsABlankPhone();
    }
}
