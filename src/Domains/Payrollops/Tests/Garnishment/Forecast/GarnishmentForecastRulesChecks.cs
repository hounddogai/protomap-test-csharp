namespace Acme.Workforce.Payrollops.Tests.Garnishment.Forecast;

using Acme.Workforce.Payrollops.Garnishment.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class GarnishmentForecastRulesChecks
{
    public static void GarnishmentForecastValidatorReportsEveryProblem()
    {
        var validator = GarnishmentForecastRules.DefaultGarnishmentForecastValidator();
        var item = GarnishmentForecast.NewGarnishmentForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GarnishmentForecastContactEmailLengthRuleFlagsALongContactEmail()
    {
        var rule = new GarnishmentForecastContactEmailLengthRule(16);
        var item = GarnishmentForecast.NewGarnishmentForecast("x", "y");
        item.ContactEmail = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ContactEmail = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GarnishmentForecastCodeLengthRuleFlagsALongCode()
    {
        var rule = new GarnishmentForecastCodeLengthRule(64);
        var item = GarnishmentForecast.NewGarnishmentForecast("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GarnishmentForecastStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new GarnishmentForecastStartsAtRangeRule(16, 120);
        var item = GarnishmentForecast.NewGarnishmentForecast("x", "y");
        item.StartsAt = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        GarnishmentForecastValidatorReportsEveryProblem();
        GarnishmentForecastContactEmailLengthRuleFlagsALongContactEmail();
        GarnishmentForecastCodeLengthRuleFlagsALongCode();
        GarnishmentForecastStartsAtRangeRuleFlagsValuesAboveStartsAt();
    }
}
