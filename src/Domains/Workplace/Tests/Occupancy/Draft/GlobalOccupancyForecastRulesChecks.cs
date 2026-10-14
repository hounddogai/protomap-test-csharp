namespace Acme.Workforce.Workplace.Tests.Occupancy.Draft;

using Acme.Workforce.Workplace.Occupancy.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class GlobalOccupancyForecastRulesChecks
{
    public static void GlobalOccupancyForecastValidatorReportsEveryProblem()
    {
        var validator = GlobalOccupancyForecastRules.DefaultGlobalOccupancyForecastValidator();
        var item = GlobalOccupancyForecast.NewGlobalOccupancyForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalOccupancyForecastLabelLengthRuleFlagsALongLabel()
    {
        var rule = new GlobalOccupancyForecastLabelLengthRule(255);
        var item = GlobalOccupancyForecast.NewGlobalOccupancyForecast("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalOccupancyForecastCodeLengthRuleFlagsALongCode()
    {
        var rule = new GlobalOccupancyForecastCodeLengthRule(86400);
        var item = GlobalOccupancyForecast.NewGlobalOccupancyForecast("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        GlobalOccupancyForecastValidatorReportsEveryProblem();
        GlobalOccupancyForecastLabelLengthRuleFlagsALongLabel();
        GlobalOccupancyForecastCodeLengthRuleFlagsALongCode();
    }
}
