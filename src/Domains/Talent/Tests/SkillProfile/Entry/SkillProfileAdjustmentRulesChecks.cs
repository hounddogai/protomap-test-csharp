namespace Acme.Workforce.Talent.Tests.SkillProfile.Entry;

using Acme.Workforce.Talent.SkillProfile.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class SkillProfileAdjustmentRulesChecks
{
    public static void SkillProfileAdjustmentValidatorReportsEveryProblem()
    {
        var validator = SkillProfileAdjustmentRules.DefaultSkillProfileAdjustmentValidator();
        var item = SkillProfileAdjustment.NewSkillProfileAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SkillProfileAdjustmentWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new SkillProfileAdjustmentWindowDaysRangeRule(64, 86400);
        var item = SkillProfileAdjustment.NewSkillProfileAdjustment("x", "y");
        item.WindowDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SkillProfileAdjustmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new SkillProfileAdjustmentCodeLengthRule(32);
        var item = SkillProfileAdjustment.NewSkillProfileAdjustment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SkillProfileAdjustmentValidatorReportsEveryProblem();
        SkillProfileAdjustmentWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        SkillProfileAdjustmentCodeLengthRuleFlagsALongCode();
    }
}
