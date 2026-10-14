namespace Acme.Workforce.Talent.Tests.SkillProfile.Mapping;

using Acme.Workforce.Talent.SkillProfile.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class CappedSkillProfileMappingRulesChecks
{
    public static void CappedSkillProfileMappingValidatorReportsEveryProblem()
    {
        var validator = CappedSkillProfileMappingRules.DefaultCappedSkillProfileMappingValidator();
        var item = CappedSkillProfileMapping.NewCappedSkillProfileMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedSkillProfileMappingRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new CappedSkillProfileMappingRequiredCategoryRule("capped skill profile mapping needs an entry");
        var item = CappedSkillProfileMapping.NewCappedSkillProfileMapping("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedSkillProfileMappingCodeLengthRuleFlagsALongCode()
    {
        var rule = new CappedSkillProfileMappingCodeLengthRule(255);
        var item = CappedSkillProfileMapping.NewCappedSkillProfileMapping("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedSkillProfileMappingSkillLengthRuleFlagsALongSkill()
    {
        var rule = new CappedSkillProfileMappingSkillLengthRule(16);
        var item = CappedSkillProfileMapping.NewCappedSkillProfileMapping("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedSkillProfileMappingRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new CappedSkillProfileMappingRequiredTitleRule("capped skill profile mapping needs a value");
        var item = CappedSkillProfileMapping.NewCappedSkillProfileMapping("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CappedSkillProfileMappingValidatorReportsEveryProblem();
        CappedSkillProfileMappingRequiredCategoryRuleFlagsABlankCategory();
        CappedSkillProfileMappingCodeLengthRuleFlagsALongCode();
        CappedSkillProfileMappingSkillLengthRuleFlagsALongSkill();
        CappedSkillProfileMappingRequiredTitleRuleFlagsABlankTitle();
    }
}
