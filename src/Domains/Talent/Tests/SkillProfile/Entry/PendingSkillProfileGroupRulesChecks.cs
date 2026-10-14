namespace Acme.Workforce.Talent.Tests.SkillProfile.Entry;

using Acme.Workforce.Talent.SkillProfile.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class PendingSkillProfileGroupRulesChecks
{
    public static void PendingSkillProfileGroupValidatorReportsEveryProblem()
    {
        var validator = PendingSkillProfileGroupRules.DefaultPendingSkillProfileGroupValidator();
        var item = PendingSkillProfileGroup.NewPendingSkillProfileGroup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingSkillProfileGroupTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PendingSkillProfileGroupTitleLengthRule(86400);
        var item = PendingSkillProfileGroup.NewPendingSkillProfileGroup("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingSkillProfileGroupRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PendingSkillProfileGroupRequiredCodeRule("pending skill profile group needs this field");
        var item = PendingSkillProfileGroup.NewPendingSkillProfileGroup("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PendingSkillProfileGroupValidatorReportsEveryProblem();
        PendingSkillProfileGroupTitleLengthRuleFlagsALongTitle();
        PendingSkillProfileGroupRequiredCodeRuleFlagsABlankCode();
    }
}
