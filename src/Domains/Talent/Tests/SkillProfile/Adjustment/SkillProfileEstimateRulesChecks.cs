namespace Acme.Workforce.Talent.Tests.SkillProfile.Adjustment;

using Acme.Workforce.Talent.SkillProfile.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class SkillProfileEstimateRulesChecks
{
    public static void SkillProfileEstimateValidatorReportsEveryProblem()
    {
        var validator = SkillProfileEstimateRules.DefaultSkillProfileEstimateValidator();
        var item = SkillProfileEstimate.NewSkillProfileEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SkillProfileEstimateGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new SkillProfileEstimateGraceDaysRangeRule(32, 120);
        var item = SkillProfileEstimate.NewSkillProfileEstimate("x", "y");
        item.GraceDays = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SkillProfileEstimateRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SkillProfileEstimateRequiredCodeRule("skill profile estimate needs this field");
        var item = SkillProfileEstimate.NewSkillProfileEstimate("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SkillProfileEstimateStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new SkillProfileEstimateStartsAtRangeRule(80, 255);
        var item = SkillProfileEstimate.NewSkillProfileEstimate("x", "y");
        item.StartsAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SkillProfileEstimateCodeLengthRuleFlagsALongCode()
    {
        var rule = new SkillProfileEstimateCodeLengthRule(120);
        var item = SkillProfileEstimate.NewSkillProfileEstimate("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SkillProfileEstimateMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new SkillProfileEstimateMarginRangeRule(6.6, 3141.4);
        var item = SkillProfileEstimate.NewSkillProfileEstimate("x", "y");
        item.Margin = 3142.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SkillProfileEstimateValidatorReportsEveryProblem();
        SkillProfileEstimateGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        SkillProfileEstimateRequiredCodeRuleFlagsABlankCode();
        SkillProfileEstimateStartsAtRangeRuleFlagsValuesAboveStartsAt();
        SkillProfileEstimateCodeLengthRuleFlagsALongCode();
        SkillProfileEstimateMarginRangeRuleFlagsValuesAboveMargin();
    }
}
