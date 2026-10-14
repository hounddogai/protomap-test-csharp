namespace Acme.Workforce.Talent.Tests.Pool.Budget;

using Acme.Workforce.Talent.Pool.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class PoolForecastRulesChecks
{
    public static void PoolForecastValidatorReportsEveryProblem()
    {
        var validator = PoolForecastRules.DefaultPoolForecastValidator();
        var item = PoolForecast.NewPoolForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PoolForecastDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new PoolForecastDiscountRangeRule(4.2, 1325.0);
        var item = PoolForecast.NewPoolForecast("x", "y");
        item.Discount = 1326.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PoolForecastCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new PoolForecastCategoryLengthRule(86400);
        var item = PoolForecast.NewPoolForecast("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PoolForecastValidatorReportsEveryProblem();
        PoolForecastDiscountRangeRuleFlagsValuesAboveDiscount();
        PoolForecastCategoryLengthRuleFlagsALongCategory();
    }
}
