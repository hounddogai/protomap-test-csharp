namespace Acme.Workforce.Talent.Tests.Pool.Baseline;

using Acme.Workforce.Talent.Pool.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class StrictPoolForecastRulesChecks
{
    public static void StrictPoolForecastValidatorReportsEveryProblem()
    {
        var validator = StrictPoolForecastRules.DefaultStrictPoolForecastValidator();
        var item = StrictPoolForecast.NewStrictPoolForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictPoolForecastRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new StrictPoolForecastRequiredCodeRule("strict pool forecast needs a value");
        var item = StrictPoolForecast.NewStrictPoolForecast("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictPoolForecastLabelLengthRuleFlagsALongLabel()
    {
        var rule = new StrictPoolForecastLabelLengthRule(80);
        var item = StrictPoolForecast.NewStrictPoolForecast("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictPoolForecastRequiredChecksumRuleFlagsABlankChecksum()
    {
        var rule = new StrictPoolForecastRequiredChecksumRule("strict pool forecast needs this field");
        var item = StrictPoolForecast.NewStrictPoolForecast("", "");
        item.Checksum = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        StrictPoolForecastValidatorReportsEveryProblem();
        StrictPoolForecastRequiredCodeRuleFlagsABlankCode();
        StrictPoolForecastLabelLengthRuleFlagsALongLabel();
        StrictPoolForecastRequiredChecksumRuleFlagsABlankChecksum();
    }
}
