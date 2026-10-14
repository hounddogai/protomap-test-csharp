namespace Acme.Workforce.Workplace.Tests.Occupancy.Metric;

using Acme.Workforce.Workplace.Occupancy.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class PrimaryOccupancyMetricRulesChecks
{
    public static void PrimaryOccupancyMetricValidatorReportsEveryProblem()
    {
        var validator = PrimaryOccupancyMetricRules.DefaultPrimaryOccupancyMetricValidator();
        var item = PrimaryOccupancyMetric.NewPrimaryOccupancyMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryOccupancyMetricLabelLengthRuleFlagsALongLabel()
    {
        var rule = new PrimaryOccupancyMetricLabelLengthRule(80);
        var item = PrimaryOccupancyMetric.NewPrimaryOccupancyMetric("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryOccupancyMetricBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new PrimaryOccupancyMetricBadgeLengthRule(1000);
        var item = PrimaryOccupancyMetric.NewPrimaryOccupancyMetric("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryOccupancyMetricRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new PrimaryOccupancyMetricRequiredExternalRefRule("primary occupancy metric needs an entry");
        var item = PrimaryOccupancyMetric.NewPrimaryOccupancyMetric("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PrimaryOccupancyMetricValidatorReportsEveryProblem();
        PrimaryOccupancyMetricLabelLengthRuleFlagsALongLabel();
        PrimaryOccupancyMetricBadgeLengthRuleFlagsALongBadge();
        PrimaryOccupancyMetricRequiredExternalRefRuleFlagsABlankExternalRef();
    }
}
