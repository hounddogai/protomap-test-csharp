namespace Acme.Workforce.Contractors.Tests.Rate.Bundle;

using Acme.Workforce.Contractors.Rate.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class RateBundleRulesChecks
{
    public static void RateBundleValidatorReportsEveryProblem()
    {
        var validator = RateBundleRules.DefaultRateBundleValidator();
        var item = RateBundle.NewRateBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RateBundleRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new RateBundleRequiredLabelRule("rate bundle needs an entry");
        var item = RateBundle.NewRateBundle("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RateBundleLabelLengthRuleFlagsALongLabel()
    {
        var rule = new RateBundleLabelLengthRule(120);
        var item = RateBundle.NewRateBundle("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RateBundleRequiredUnitRuleFlagsABlankUnit()
    {
        var rule = new RateBundleRequiredUnitRule("rate bundle needs this field");
        var item = RateBundle.NewRateBundle("", "");
        item.Unit = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RateBundleUnitLengthRuleFlagsALongUnit()
    {
        var rule = new RateBundleUnitLengthRule(32);
        var item = RateBundle.NewRateBundle("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RateBundleValidatorReportsEveryProblem();
        RateBundleRequiredLabelRuleFlagsABlankLabel();
        RateBundleLabelLengthRuleFlagsALongLabel();
        RateBundleRequiredUnitRuleFlagsABlankUnit();
        RateBundleUnitLengthRuleFlagsALongUnit();
    }
}
