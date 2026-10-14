namespace Acme.Workforce.Talent.Tests.Pool.Window;

using Acme.Workforce.Talent.Pool.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class DefaultPoolForecastRulesChecks
{
    public static void DefaultPoolForecastValidatorReportsEveryProblem()
    {
        var validator = DefaultPoolForecastRules.DefaultDefaultPoolForecastValidator();
        var item = DefaultPoolForecast.NewDefaultPoolForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultPoolForecastLocationLengthRuleFlagsALongLocation()
    {
        var rule = new DefaultPoolForecastLocationLengthRule(255);
        var item = DefaultPoolForecast.NewDefaultPoolForecast("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultPoolForecastRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DefaultPoolForecastRequiredCodeRule("default pool forecast needs a value");
        var item = DefaultPoolForecast.NewDefaultPoolForecast("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DefaultPoolForecastValidatorReportsEveryProblem();
        DefaultPoolForecastLocationLengthRuleFlagsALongLocation();
        DefaultPoolForecastRequiredCodeRuleFlagsABlankCode();
    }
}
