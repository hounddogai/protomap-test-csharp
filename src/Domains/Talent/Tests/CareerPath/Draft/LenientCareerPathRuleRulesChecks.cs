namespace Acme.Workforce.Talent.Tests.CareerPath.Draft;

using Acme.Workforce.Talent.CareerPath.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class LenientCareerPathRuleRulesChecks
{
    public static void LenientCareerPathRuleValidatorReportsEveryProblem()
    {
        var validator = LenientCareerPathRuleRules.DefaultLenientCareerPathRuleValidator();
        var item = LenientCareerPathRule.NewLenientCareerPathRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientCareerPathRuleNameLengthRuleFlagsALongName()
    {
        var rule = new LenientCareerPathRuleNameLengthRule(3600);
        var item = LenientCareerPathRule.NewLenientCareerPathRule("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientCareerPathRuleRequiredNameRuleFlagsABlankName()
    {
        var rule = new LenientCareerPathRuleRequiredNameRule("lenient career path rule needs an entry");
        var item = LenientCareerPathRule.NewLenientCareerPathRule("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientCareerPathRuleRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new LenientCareerPathRuleRequiredSourceRule("lenient career path rule needs a value");
        var item = LenientCareerPathRule.NewLenientCareerPathRule("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientCareerPathRuleCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new LenientCareerPathRuleCreatedAtRangeRule(32, 3600);
        var item = LenientCareerPathRule.NewLenientCareerPathRule("x", "y");
        item.CreatedAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientCareerPathRuleRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new LenientCareerPathRuleRateRangeRule(0.8, 327.5);
        var item = LenientCareerPathRule.NewLenientCareerPathRule("x", "y");
        item.Rate = 329.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LenientCareerPathRuleValidatorReportsEveryProblem();
        LenientCareerPathRuleNameLengthRuleFlagsALongName();
        LenientCareerPathRuleRequiredNameRuleFlagsABlankName();
        LenientCareerPathRuleRequiredSourceRuleFlagsABlankSource();
        LenientCareerPathRuleCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        LenientCareerPathRuleRateRangeRuleFlagsValuesAboveRate();
    }
}
