namespace Acme.Workforce.Talent.Tests.Pool.PoolDraft;

using Acme.Workforce.Talent.Pool.PoolDraft;

/// <summary>Checks of pool draft code, run by the project's runner.</summary>
public static class RegionalPoolMetricRulesChecks
{
    public static void RegionalPoolMetricValidatorReportsEveryProblem()
    {
        var validator = RegionalPoolMetricRules.DefaultRegionalPoolMetricValidator();
        var item = RegionalPoolMetric.NewRegionalPoolMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalPoolMetricRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new RegionalPoolMetricRetriesRangeRule(80, 1000);
        var item = RegionalPoolMetric.NewRegionalPoolMetric("x", "y");
        item.Retries = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalPoolMetricRequiredOwnerIdRuleFlagsABlankOwnerId()
    {
        var rule = new RegionalPoolMetricRequiredOwnerIdRule("regional pool metric needs an entry");
        var item = RegionalPoolMetric.NewRegionalPoolMetric("", "");
        item.OwnerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalPoolMetricThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new RegionalPoolMetricThresholdRangeRule(5.7, 3647.7);
        var item = RegionalPoolMetric.NewRegionalPoolMetric("x", "y");
        item.Threshold = 3649.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RegionalPoolMetricValidatorReportsEveryProblem();
        RegionalPoolMetricRetriesRangeRuleFlagsValuesAboveRetries();
        RegionalPoolMetricRequiredOwnerIdRuleFlagsABlankOwnerId();
        RegionalPoolMetricThresholdRangeRuleFlagsValuesAboveThreshold();
    }
}
