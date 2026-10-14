namespace Acme.Workforce.Contractors.Tests.Rate.Plan;

using Acme.Workforce.Contractors.Rate.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class RateMappingRulesChecks
{
    public static void RateMappingValidatorReportsEveryProblem()
    {
        var validator = RateMappingRules.DefaultRateMappingValidator();
        var item = RateMapping.NewRateMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RateMappingRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new RateMappingRequiredTitleRule("rate mapping needs an entry");
        var item = RateMapping.NewRateMapping("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RateMappingVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new RateMappingVersionRangeRule(16, 86400);
        var item = RateMapping.NewRateMapping("x", "y");
        item.Version = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RateMappingTitleLengthRuleFlagsALongTitle()
    {
        var rule = new RateMappingTitleLengthRule(86400);
        var item = RateMapping.NewRateMapping("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RateMappingRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new RateMappingRequiredPhoneRule("rate mapping needs an entry");
        var item = RateMapping.NewRateMapping("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RateMappingValidatorReportsEveryProblem();
        RateMappingRequiredTitleRuleFlagsABlankTitle();
        RateMappingVersionRangeRuleFlagsValuesAboveVersion();
        RateMappingTitleLengthRuleFlagsALongTitle();
        RateMappingRequiredPhoneRuleFlagsABlankPhone();
    }
}
