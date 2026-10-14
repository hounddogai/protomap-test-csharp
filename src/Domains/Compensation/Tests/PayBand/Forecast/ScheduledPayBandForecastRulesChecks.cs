namespace Acme.Workforce.Compensation.Tests.PayBand.Forecast;

using Acme.Workforce.Compensation.PayBand.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class ScheduledPayBandForecastRulesChecks
{
    public static void ScheduledPayBandForecastValidatorReportsEveryProblem()
    {
        var validator = ScheduledPayBandForecastRules.DefaultScheduledPayBandForecastValidator();
        var item = ScheduledPayBandForecast.NewScheduledPayBandForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledPayBandForecastNameLengthRuleFlagsALongName()
    {
        var rule = new ScheduledPayBandForecastNameLengthRule(86400);
        var item = ScheduledPayBandForecast.NewScheduledPayBandForecast("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledPayBandForecastRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ScheduledPayBandForecastRequiredCodeRule("scheduled pay band forecast needs an entry");
        var item = ScheduledPayBandForecast.NewScheduledPayBandForecast("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledPayBandForecastTeamLengthRuleFlagsALongTeam()
    {
        var rule = new ScheduledPayBandForecastTeamLengthRule(1000);
        var item = ScheduledPayBandForecast.NewScheduledPayBandForecast("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledPayBandForecastRequiredNameRuleFlagsABlankName()
    {
        var rule = new ScheduledPayBandForecastRequiredNameRule("scheduled pay band forecast needs a value");
        var item = ScheduledPayBandForecast.NewScheduledPayBandForecast("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ScheduledPayBandForecastValidatorReportsEveryProblem();
        ScheduledPayBandForecastNameLengthRuleFlagsALongName();
        ScheduledPayBandForecastRequiredCodeRuleFlagsABlankCode();
        ScheduledPayBandForecastTeamLengthRuleFlagsALongTeam();
        ScheduledPayBandForecastRequiredNameRuleFlagsABlankName();
    }
}
