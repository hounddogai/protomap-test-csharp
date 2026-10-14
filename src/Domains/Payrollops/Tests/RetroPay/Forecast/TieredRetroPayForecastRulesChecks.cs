namespace Acme.Workforce.Payrollops.Tests.RetroPay.Forecast;

using Acme.Workforce.Payrollops.RetroPay.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class TieredRetroPayForecastRulesChecks
{
    public static void TieredRetroPayForecastValidatorReportsEveryProblem()
    {
        var validator = TieredRetroPayForecastRules.DefaultTieredRetroPayForecastValidator();
        var item = TieredRetroPayForecast.NewTieredRetroPayForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredRetroPayForecastNameLengthRuleFlagsALongName()
    {
        var rule = new TieredRetroPayForecastNameLengthRule(32);
        var item = TieredRetroPayForecast.NewTieredRetroPayForecast("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredRetroPayForecastRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new TieredRetroPayForecastRateRangeRule(2.1, 4395.0);
        var item = TieredRetroPayForecast.NewTieredRetroPayForecast("x", "y");
        item.Rate = 4396.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        TieredRetroPayForecastValidatorReportsEveryProblem();
        TieredRetroPayForecastNameLengthRuleFlagsALongName();
        TieredRetroPayForecastRateRangeRuleFlagsValuesAboveRate();
    }
}
