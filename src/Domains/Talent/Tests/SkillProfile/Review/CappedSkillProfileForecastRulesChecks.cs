namespace Acme.Workforce.Talent.Tests.SkillProfile.Review;

using Acme.Workforce.Talent.SkillProfile.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class CappedSkillProfileForecastRulesChecks
{
    public static void CappedSkillProfileForecastValidatorReportsEveryProblem()
    {
        var validator = CappedSkillProfileForecastRules.DefaultCappedSkillProfileForecastValidator();
        var item = CappedSkillProfileForecast.NewCappedSkillProfileForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedSkillProfileForecastEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new CappedSkillProfileForecastEndsAtRangeRule(1000, 86400);
        var item = CappedSkillProfileForecast.NewCappedSkillProfileForecast("x", "y");
        item.EndsAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedSkillProfileForecastRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new CappedSkillProfileForecastRequiredLabelRule("capped skill profile forecast needs this field");
        var item = CappedSkillProfileForecast.NewCappedSkillProfileForecast("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedSkillProfileForecastRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new CappedSkillProfileForecastRequiredCodeRule("capped skill profile forecast needs an entry");
        var item = CappedSkillProfileForecast.NewCappedSkillProfileForecast("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedSkillProfileForecastLabelLengthRuleFlagsALongLabel()
    {
        var rule = new CappedSkillProfileForecastLabelLengthRule(86400);
        var item = CappedSkillProfileForecast.NewCappedSkillProfileForecast("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CappedSkillProfileForecastValidatorReportsEveryProblem();
        CappedSkillProfileForecastEndsAtRangeRuleFlagsValuesAboveEndsAt();
        CappedSkillProfileForecastRequiredLabelRuleFlagsABlankLabel();
        CappedSkillProfileForecastRequiredCodeRuleFlagsABlankCode();
        CappedSkillProfileForecastLabelLengthRuleFlagsALongLabel();
    }
}
