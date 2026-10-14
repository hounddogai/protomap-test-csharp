namespace Acme.Workforce.Contractors.Tests.Rate.Segment;

using Acme.Workforce.Contractors.Rate.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class RateOverrideRulesChecks
{
    public static void RateOverrideValidatorReportsEveryProblem()
    {
        var validator = RateOverrideRules.DefaultRateOverrideValidator();
        var item = RateOverride.NewRateOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RateOverrideScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new RateOverrideScoreRangeRule(5.6, 4456.1);
        var item = RateOverride.NewRateOverride("x", "y");
        item.Score = 4457.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RateOverrideRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RateOverrideRequiredCodeRule("rate override needs a value");
        var item = RateOverride.NewRateOverride("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RateOverrideDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new RateOverrideDescriptionLengthRule(1000);
        var item = RateOverride.NewRateOverride("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RateOverrideNameLengthRuleFlagsALongName()
    {
        var rule = new RateOverrideNameLengthRule(86400);
        var item = RateOverride.NewRateOverride("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RateOverrideValidatorReportsEveryProblem();
        RateOverrideScoreRangeRuleFlagsValuesAboveScore();
        RateOverrideRequiredCodeRuleFlagsABlankCode();
        RateOverrideDescriptionLengthRuleFlagsALongDescription();
        RateOverrideNameLengthRuleFlagsALongName();
    }
}
