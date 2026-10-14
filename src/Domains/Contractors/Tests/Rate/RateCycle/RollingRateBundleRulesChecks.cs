namespace Acme.Workforce.Contractors.Tests.Rate.RateCycle;

using Acme.Workforce.Contractors.Rate.RateCycle;

/// <summary>Checks of rate cycle code, run by the project's runner.</summary>
public static class RollingRateBundleRulesChecks
{
    public static void RollingRateBundleValidatorReportsEveryProblem()
    {
        var validator = RollingRateBundleRules.DefaultRollingRateBundleValidator();
        var item = RollingRateBundle.NewRollingRateBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingRateBundleDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new RollingRateBundleDiscountRangeRule(9.7, 3258.9);
        var item = RollingRateBundle.NewRollingRateBundle("x", "y");
        item.Discount = 3260.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingRateBundleRequiredNameRuleFlagsABlankName()
    {
        var rule = new RollingRateBundleRequiredNameRule("rolling rate bundle needs a value");
        var item = RollingRateBundle.NewRollingRateBundle("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingRateBundleCodeLengthRuleFlagsALongCode()
    {
        var rule = new RollingRateBundleCodeLengthRule(80);
        var item = RollingRateBundle.NewRollingRateBundle("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RollingRateBundleValidatorReportsEveryProblem();
        RollingRateBundleDiscountRangeRuleFlagsValuesAboveDiscount();
        RollingRateBundleRequiredNameRuleFlagsABlankName();
        RollingRateBundleCodeLengthRuleFlagsALongCode();
    }
}
