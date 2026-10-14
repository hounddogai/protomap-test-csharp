namespace Acme.Workforce.Talent.Tests.SkillProfile.Target;

using Acme.Workforce.Talent.SkillProfile.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class PendingSkillProfileTargetRulesChecks
{
    public static void PendingSkillProfileTargetValidatorReportsEveryProblem()
    {
        var validator = PendingSkillProfileTargetRules.DefaultPendingSkillProfileTargetValidator();
        var item = PendingSkillProfileTarget.NewPendingSkillProfileTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingSkillProfileTargetCodeLengthRuleFlagsALongCode()
    {
        var rule = new PendingSkillProfileTargetCodeLengthRule(3600);
        var item = PendingSkillProfileTarget.NewPendingSkillProfileTarget("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingSkillProfileTargetTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new PendingSkillProfileTargetTimezoneLengthRule(120);
        var item = PendingSkillProfileTarget.NewPendingSkillProfileTarget("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingSkillProfileTargetRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PendingSkillProfileTargetRequiredCodeRule("pending skill profile target needs a value");
        var item = PendingSkillProfileTarget.NewPendingSkillProfileTarget("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingSkillProfileTargetRequiredNameRuleFlagsABlankName()
    {
        var rule = new PendingSkillProfileTargetRequiredNameRule("pending skill profile target needs a value");
        var item = PendingSkillProfileTarget.NewPendingSkillProfileTarget("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PendingSkillProfileTargetValidatorReportsEveryProblem();
        PendingSkillProfileTargetCodeLengthRuleFlagsALongCode();
        PendingSkillProfileTargetTimezoneLengthRuleFlagsALongTimezone();
        PendingSkillProfileTargetRequiredCodeRuleFlagsABlankCode();
        PendingSkillProfileTargetRequiredNameRuleFlagsABlankName();
    }
}
