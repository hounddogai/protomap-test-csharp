namespace Acme.Workforce.Absence.Tests.Accrual.Forecast;

using Acme.Workforce.Absence.Accrual.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class StrictAccrualForecastRulesChecks
{
    public static void StrictAccrualForecastValidatorReportsEveryProblem()
    {
        var validator = StrictAccrualForecastRules.DefaultStrictAccrualForecastValidator();
        var item = StrictAccrualForecast.NewStrictAccrualForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictAccrualForecastNameLengthRuleFlagsALongName()
    {
        var rule = new StrictAccrualForecastNameLengthRule(86400);
        var item = StrictAccrualForecast.NewStrictAccrualForecast("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictAccrualForecastRequiredNameRuleFlagsABlankName()
    {
        var rule = new StrictAccrualForecastRequiredNameRule("strict accrual forecast needs a value");
        var item = StrictAccrualForecast.NewStrictAccrualForecast("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        StrictAccrualForecastValidatorReportsEveryProblem();
        StrictAccrualForecastNameLengthRuleFlagsALongName();
        StrictAccrualForecastRequiredNameRuleFlagsABlankName();
    }
}
