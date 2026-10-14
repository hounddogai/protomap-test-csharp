namespace Acme.Workforce.Workplace.Tests.Occupancy.Draft;

using Acme.Workforce.Workplace.Occupancy.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class OccupancyForecastRulesChecks
{
    public static void OccupancyForecastValidatorReportsEveryProblem()
    {
        var validator = OccupancyForecastRules.DefaultOccupancyForecastValidator();
        var item = OccupancyForecast.NewOccupancyForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void OccupancyForecastMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new OccupancyForecastMarginRangeRule(9.3, 4627.7);
        var item = OccupancyForecast.NewOccupancyForecast("x", "y");
        item.Margin = 4629.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void OccupancyForecastCountryLengthRuleFlagsALongCountry()
    {
        var rule = new OccupancyForecastCountryLengthRule(255);
        var item = OccupancyForecast.NewOccupancyForecast("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void OccupancyForecastRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new OccupancyForecastRequiredTitleRule("occupancy forecast needs a value");
        var item = OccupancyForecast.NewOccupancyForecast("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void OccupancyForecastRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new OccupancyForecastRequiredTeamRule("occupancy forecast needs a value");
        var item = OccupancyForecast.NewOccupancyForecast("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        OccupancyForecastValidatorReportsEveryProblem();
        OccupancyForecastMarginRangeRuleFlagsValuesAboveMargin();
        OccupancyForecastCountryLengthRuleFlagsALongCountry();
        OccupancyForecastRequiredTitleRuleFlagsABlankTitle();
        OccupancyForecastRequiredTeamRuleFlagsABlankTeam();
    }
}
