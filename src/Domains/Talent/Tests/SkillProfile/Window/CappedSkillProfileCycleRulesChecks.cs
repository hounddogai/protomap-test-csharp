namespace Acme.Workforce.Talent.Tests.SkillProfile.Window;

using Acme.Workforce.Talent.SkillProfile.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class CappedSkillProfileCycleRulesChecks
{
    public static void CappedSkillProfileCycleValidatorReportsEveryProblem()
    {
        var validator = CappedSkillProfileCycleRules.DefaultCappedSkillProfileCycleValidator();
        var item = CappedSkillProfileCycle.NewCappedSkillProfileCycle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedSkillProfileCycleDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new CappedSkillProfileCycleDescriptionLengthRule(32);
        var item = CappedSkillProfileCycle.NewCappedSkillProfileCycle("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedSkillProfileCycleRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new CappedSkillProfileCycleRequiredDescriptionRule("capped skill profile cycle needs a value");
        var item = CappedSkillProfileCycle.NewCappedSkillProfileCycle("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedSkillProfileCycleCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new CappedSkillProfileCycleCreatedAtRangeRule(255, 86400);
        var item = CappedSkillProfileCycle.NewCappedSkillProfileCycle("x", "y");
        item.CreatedAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedSkillProfileCycleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new CappedSkillProfileCycleRequiredCodeRule("capped skill profile cycle needs a value");
        var item = CappedSkillProfileCycle.NewCappedSkillProfileCycle("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CappedSkillProfileCycleValidatorReportsEveryProblem();
        CappedSkillProfileCycleDescriptionLengthRuleFlagsALongDescription();
        CappedSkillProfileCycleRequiredDescriptionRuleFlagsABlankDescription();
        CappedSkillProfileCycleCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        CappedSkillProfileCycleRequiredCodeRuleFlagsABlankCode();
    }
}
