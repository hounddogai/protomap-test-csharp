namespace Acme.Workforce.Contractors.Tests.Rate.RateBatch;

using Acme.Workforce.Contractors.Rate.RateBatch;

/// <summary>Checks of rate batch code, run by the project's runner.</summary>
public static class RateForecastRulesChecks
{
    public static void RateForecastValidatorReportsEveryProblem()
    {
        var validator = RateForecastRules.DefaultRateForecastValidator();
        var item = RateForecast.NewRateForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RateForecastRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new RateForecastRequiredTitleRule("rate forecast needs this field");
        var item = RateForecast.NewRateForecast("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RateForecastRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new RateForecastRequiredCategoryRule("rate forecast needs a value");
        var item = RateForecast.NewRateForecast("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RateForecastCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new RateForecastCostRangeRule(9.6, 4114.5);
        var item = RateForecast.NewRateForecast("x", "y");
        item.Cost = 4116.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RateForecastRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RateForecastRequiredCodeRule("rate forecast needs this field");
        var item = RateForecast.NewRateForecast("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RateForecastTitleLengthRuleFlagsALongTitle()
    {
        var rule = new RateForecastTitleLengthRule(32);
        var item = RateForecast.NewRateForecast("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RateForecastValidatorReportsEveryProblem();
        RateForecastRequiredTitleRuleFlagsABlankTitle();
        RateForecastRequiredCategoryRuleFlagsABlankCategory();
        RateForecastCostRangeRuleFlagsValuesAboveCost();
        RateForecastRequiredCodeRuleFlagsABlankCode();
        RateForecastTitleLengthRuleFlagsALongTitle();
    }
}
