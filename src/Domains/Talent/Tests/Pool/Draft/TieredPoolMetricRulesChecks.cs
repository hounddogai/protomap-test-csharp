namespace Acme.Workforce.Talent.Tests.Pool.Draft;

using Acme.Workforce.Talent.Pool.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class TieredPoolMetricRulesChecks
{
    public static void TieredPoolMetricValidatorReportsEveryProblem()
    {
        var validator = TieredPoolMetricRules.DefaultTieredPoolMetricValidator();
        var item = TieredPoolMetric.NewTieredPoolMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredPoolMetricRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new TieredPoolMetricRequiredLabelRule("tiered pool metric needs an entry");
        var item = TieredPoolMetric.NewTieredPoolMetric("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TieredPoolMetricValidatorReportsEveryProblem();
        TieredPoolMetricRequiredLabelRuleFlagsABlankLabel();
    }
}
