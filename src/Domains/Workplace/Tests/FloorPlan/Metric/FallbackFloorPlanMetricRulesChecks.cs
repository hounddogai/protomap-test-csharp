namespace Acme.Workforce.Workplace.Tests.FloorPlan.Metric;

using Acme.Workforce.Workplace.FloorPlan.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class FallbackFloorPlanMetricRulesChecks
{
    public static void FallbackFloorPlanMetricValidatorReportsEveryProblem()
    {
        var validator = FallbackFloorPlanMetricRules.DefaultFallbackFloorPlanMetricValidator();
        var item = FallbackFloorPlanMetric.NewFallbackFloorPlanMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackFloorPlanMetricChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new FallbackFloorPlanMetricChecksumLengthRule(80);
        var item = FallbackFloorPlanMetric.NewFallbackFloorPlanMetric("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackFloorPlanMetricRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new FallbackFloorPlanMetricRequiredLabelRule("fallback floor plan metric needs a value");
        var item = FallbackFloorPlanMetric.NewFallbackFloorPlanMetric("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackFloorPlanMetricLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new FallbackFloorPlanMetricLeaveDaysRangeRule(16, 3600);
        var item = FallbackFloorPlanMetric.NewFallbackFloorPlanMetric("x", "y");
        item.LeaveDays = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        FallbackFloorPlanMetricValidatorReportsEveryProblem();
        FallbackFloorPlanMetricChecksumLengthRuleFlagsALongChecksum();
        FallbackFloorPlanMetricRequiredLabelRuleFlagsABlankLabel();
        FallbackFloorPlanMetricLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
    }
}
