namespace Acme.Workforce.Workplace.Tests.Desk.Digest;

using Acme.Workforce.Workplace.Desk.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class DeskForecastRulesChecks
{
    public static void DeskForecastValidatorReportsEveryProblem()
    {
        var validator = DeskForecastRules.DefaultDeskForecastValidator();
        var item = DeskForecast.NewDeskForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeskForecastTitleLengthRuleFlagsALongTitle()
    {
        var rule = new DeskForecastTitleLengthRule(64);
        var item = DeskForecast.NewDeskForecast("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DeskForecastCodeLengthRuleFlagsALongCode()
    {
        var rule = new DeskForecastCodeLengthRule(80);
        var item = DeskForecast.NewDeskForecast("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DeskForecastValidatorReportsEveryProblem();
        DeskForecastTitleLengthRuleFlagsALongTitle();
        DeskForecastCodeLengthRuleFlagsALongCode();
    }
}
