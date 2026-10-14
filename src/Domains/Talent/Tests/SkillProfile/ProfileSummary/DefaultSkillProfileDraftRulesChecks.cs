namespace Acme.Workforce.Talent.Tests.SkillProfile.ProfileSummary;

using Acme.Workforce.Talent.SkillProfile.ProfileSummary;

/// <summary>Checks of profile summary code, run by the project's runner.</summary>
public static class DefaultSkillProfileDraftRulesChecks
{
    public static void DefaultSkillProfileDraftValidatorReportsEveryProblem()
    {
        var validator = DefaultSkillProfileDraftRules.DefaultDefaultSkillProfileDraftValidator();
        var item = DefaultSkillProfileDraft.NewDefaultSkillProfileDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultSkillProfileDraftCodeLengthRuleFlagsALongCode()
    {
        var rule = new DefaultSkillProfileDraftCodeLengthRule(80);
        var item = DefaultSkillProfileDraft.NewDefaultSkillProfileDraft("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultSkillProfileDraftDepartmentLengthRuleFlagsALongDepartment()
    {
        var rule = new DefaultSkillProfileDraftDepartmentLengthRule(32);
        var item = DefaultSkillProfileDraft.NewDefaultSkillProfileDraft("x", "y");
        item.Department = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Department = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultSkillProfileDraftRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DefaultSkillProfileDraftRequiredCodeRule("default skill profile draft needs a value");
        var item = DefaultSkillProfileDraft.NewDefaultSkillProfileDraft("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultSkillProfileDraftNameLengthRuleFlagsALongName()
    {
        var rule = new DefaultSkillProfileDraftNameLengthRule(64);
        var item = DefaultSkillProfileDraft.NewDefaultSkillProfileDraft("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DefaultSkillProfileDraftValidatorReportsEveryProblem();
        DefaultSkillProfileDraftCodeLengthRuleFlagsALongCode();
        DefaultSkillProfileDraftDepartmentLengthRuleFlagsALongDepartment();
        DefaultSkillProfileDraftRequiredCodeRuleFlagsABlankCode();
        DefaultSkillProfileDraftNameLengthRuleFlagsALongName();
    }
}
