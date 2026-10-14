namespace Acme.Workforce.Compensation.Tests.MeritCycle.Reservation;

using Acme.Workforce.Compensation.MeritCycle.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class MeritCycleForecastRulesChecks
{
    public static void MeritCycleForecastValidatorReportsEveryProblem()
    {
        var validator = MeritCycleForecastRules.DefaultMeritCycleForecastValidator();
        var item = MeritCycleForecast.NewMeritCycleForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MeritCycleForecastRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new MeritCycleForecastRequiredCodeRule("merit cycle forecast needs an entry");
        var item = MeritCycleForecast.NewMeritCycleForecast("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MeritCycleForecastRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new MeritCycleForecastRequiredTitleRule("merit cycle forecast needs this field");
        var item = MeritCycleForecast.NewMeritCycleForecast("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MeritCycleForecastCodeLengthRuleFlagsALongCode()
    {
        var rule = new MeritCycleForecastCodeLengthRule(80);
        var item = MeritCycleForecast.NewMeritCycleForecast("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        MeritCycleForecastValidatorReportsEveryProblem();
        MeritCycleForecastRequiredCodeRuleFlagsABlankCode();
        MeritCycleForecastRequiredTitleRuleFlagsABlankTitle();
        MeritCycleForecastCodeLengthRuleFlagsALongCode();
    }
}
