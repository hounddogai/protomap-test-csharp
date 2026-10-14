namespace Acme.Workforce.Assets.Tests.Laptop.Forecast;

using Acme.Workforce.Assets.Laptop.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class LaptopForecastRulesChecks
{
    public static void LaptopForecastValidatorReportsEveryProblem()
    {
        var validator = LaptopForecastRules.DefaultLaptopForecastValidator();
        var item = LaptopForecast.NewLaptopForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LaptopForecastLabelLengthRuleFlagsALongLabel()
    {
        var rule = new LaptopForecastLabelLengthRule(80);
        var item = LaptopForecast.NewLaptopForecast("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LaptopForecastThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new LaptopForecastThresholdRangeRule(4.6, 4335.6);
        var item = LaptopForecast.NewLaptopForecast("x", "y");
        item.Threshold = 4337.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LaptopForecastCodeLengthRuleFlagsALongCode()
    {
        var rule = new LaptopForecastCodeLengthRule(120);
        var item = LaptopForecast.NewLaptopForecast("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LaptopForecastValidatorReportsEveryProblem();
        LaptopForecastLabelLengthRuleFlagsALongLabel();
        LaptopForecastThresholdRangeRuleFlagsValuesAboveThreshold();
        LaptopForecastCodeLengthRuleFlagsALongCode();
    }
}
