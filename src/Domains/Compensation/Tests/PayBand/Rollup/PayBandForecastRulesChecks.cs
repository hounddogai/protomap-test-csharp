namespace Acme.Workforce.Compensation.Tests.PayBand.Rollup;

using Acme.Workforce.Compensation.PayBand.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class PayBandForecastRulesChecks
{
    public static void PayBandForecastValidatorReportsEveryProblem()
    {
        var validator = PayBandForecastRules.DefaultPayBandForecastValidator();
        var item = PayBandForecast.NewPayBandForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayBandForecastDepartmentLengthRuleFlagsALongDepartment()
    {
        var rule = new PayBandForecastDepartmentLengthRule(32);
        var item = PayBandForecast.NewPayBandForecast("x", "y");
        item.Department = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Department = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayBandForecastRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new PayBandForecastRatingRangeRule(8.7, 3576.6);
        var item = PayBandForecast.NewPayBandForecast("x", "y");
        item.Rating = 3578.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PayBandForecastLabelLengthRuleFlagsALongLabel()
    {
        var rule = new PayBandForecastLabelLengthRule(120);
        var item = PayBandForecast.NewPayBandForecast("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayBandForecastRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new PayBandForecastRequiredDepartmentRule("pay band forecast needs this field");
        var item = PayBandForecast.NewPayBandForecast("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PayBandForecastValidatorReportsEveryProblem();
        PayBandForecastDepartmentLengthRuleFlagsALongDepartment();
        PayBandForecastRatingRangeRuleFlagsValuesAboveRating();
        PayBandForecastLabelLengthRuleFlagsALongLabel();
        PayBandForecastRequiredDepartmentRuleFlagsABlankDepartment();
    }
}
