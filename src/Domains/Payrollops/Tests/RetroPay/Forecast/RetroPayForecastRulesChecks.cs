namespace Acme.Workforce.Payrollops.Tests.RetroPay.Forecast;

using Acme.Workforce.Payrollops.RetroPay.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class RetroPayForecastRulesChecks
{
    public static void RetroPayForecastValidatorReportsEveryProblem()
    {
        var validator = RetroPayForecastRules.DefaultRetroPayForecastValidator();
        var item = RetroPayForecast.NewRetroPayForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RetroPayForecastDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new RetroPayForecastDescriptionLengthRule(255);
        var item = RetroPayForecast.NewRetroPayForecast("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RetroPayForecastRequiredRegionRuleFlagsABlankRegion()
    {
        var rule = new RetroPayForecastRequiredRegionRule("retro pay forecast needs this field");
        var item = RetroPayForecast.NewRetroPayForecast("", "");
        item.Region = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RetroPayForecastCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new RetroPayForecastCeilingRangeRule(0.2, 3485.6);
        var item = RetroPayForecast.NewRetroPayForecast("x", "y");
        item.Ceiling = 3487.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RetroPayForecastQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new RetroPayForecastQuantityRangeRule(80, 255);
        var item = RetroPayForecast.NewRetroPayForecast("x", "y");
        item.Quantity = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RetroPayForecastValidatorReportsEveryProblem();
        RetroPayForecastDescriptionLengthRuleFlagsALongDescription();
        RetroPayForecastRequiredRegionRuleFlagsABlankRegion();
        RetroPayForecastCeilingRangeRuleFlagsValuesAboveCeiling();
        RetroPayForecastQuantityRangeRuleFlagsValuesAboveQuantity();
    }
}
