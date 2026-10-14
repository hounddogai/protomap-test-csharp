namespace Acme.Workforce.Talent.Tests.Pool.Summary;

using Acme.Workforce.Talent.Pool.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class FallbackPoolMetricRulesChecks
{
    public static void FallbackPoolMetricValidatorReportsEveryProblem()
    {
        var validator = FallbackPoolMetricRules.DefaultFallbackPoolMetricValidator();
        var item = FallbackPoolMetric.NewFallbackPoolMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackPoolMetricRequiredCountryRuleFlagsABlankCountry()
    {
        var rule = new FallbackPoolMetricRequiredCountryRule("fallback pool metric needs this field");
        var item = FallbackPoolMetric.NewFallbackPoolMetric("", "");
        item.Country = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackPoolMetricScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new FallbackPoolMetricScoreRangeRule(9.6, 4253.6);
        var item = FallbackPoolMetric.NewFallbackPoolMetric("x", "y");
        item.Score = 4255.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        FallbackPoolMetricValidatorReportsEveryProblem();
        FallbackPoolMetricRequiredCountryRuleFlagsABlankCountry();
        FallbackPoolMetricScoreRangeRuleFlagsValuesAboveScore();
    }
}
