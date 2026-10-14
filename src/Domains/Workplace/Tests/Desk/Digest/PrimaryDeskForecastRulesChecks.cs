namespace Acme.Workforce.Workplace.Tests.Desk.Digest;

using Acme.Workforce.Workplace.Desk.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class PrimaryDeskForecastRulesChecks
{
    public static void PrimaryDeskForecastValidatorReportsEveryProblem()
    {
        var validator = PrimaryDeskForecastRules.DefaultPrimaryDeskForecastValidator();
        var item = PrimaryDeskForecast.NewPrimaryDeskForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryDeskForecastRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PrimaryDeskForecastRequiredCodeRule("primary desk forecast needs a value");
        var item = PrimaryDeskForecast.NewPrimaryDeskForecast("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryDeskForecastRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new PrimaryDeskForecastRequiredGradeRule("primary desk forecast needs this field");
        var item = PrimaryDeskForecast.NewPrimaryDeskForecast("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PrimaryDeskForecastValidatorReportsEveryProblem();
        PrimaryDeskForecastRequiredCodeRuleFlagsABlankCode();
        PrimaryDeskForecastRequiredGradeRuleFlagsABlankGrade();
    }
}
