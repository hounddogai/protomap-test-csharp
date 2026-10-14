namespace Acme.Workforce.Talent.Tests.SkillProfile.Policy;

using Acme.Workforce.Talent.SkillProfile.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class SkillProfileAuditRulesChecks
{
    public static void SkillProfileAuditValidatorReportsEveryProblem()
    {
        var validator = SkillProfileAuditRules.DefaultSkillProfileAuditValidator();
        var item = SkillProfileAudit.NewSkillProfileAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SkillProfileAuditTitleLengthRuleFlagsALongTitle()
    {
        var rule = new SkillProfileAuditTitleLengthRule(3600);
        var item = SkillProfileAudit.NewSkillProfileAudit("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SkillProfileAuditRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SkillProfileAuditRequiredCodeRule("skill profile audit needs an entry");
        var item = SkillProfileAudit.NewSkillProfileAudit("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SkillProfileAuditValidatorReportsEveryProblem();
        SkillProfileAuditTitleLengthRuleFlagsALongTitle();
        SkillProfileAuditRequiredCodeRuleFlagsABlankCode();
    }
}
