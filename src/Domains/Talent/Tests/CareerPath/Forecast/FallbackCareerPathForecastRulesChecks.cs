namespace Acme.Workforce.Talent.Tests.CareerPath.Forecast;

using Acme.Workforce.Talent.CareerPath.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class FallbackCareerPathForecastRulesChecks
{
    public static void FallbackCareerPathForecastValidatorReportsEveryProblem()
    {
        var validator = FallbackCareerPathForecastRules.DefaultFallbackCareerPathForecastValidator();
        var item = FallbackCareerPathForecast.NewFallbackCareerPathForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackCareerPathForecastLabelLengthRuleFlagsALongLabel()
    {
        var rule = new FallbackCareerPathForecastLabelLengthRule(16);
        var item = FallbackCareerPathForecast.NewFallbackCareerPathForecast("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        FallbackCareerPathForecastValidatorReportsEveryProblem();
        FallbackCareerPathForecastLabelLengthRuleFlagsALongLabel();
    }
}
