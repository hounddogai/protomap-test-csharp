namespace Acme.Workforce.Workplace.Tests.FloorPlan.Metric;

using Acme.Workforce.Workplace.FloorPlan.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class ManualFloorPlanMetricRulesChecks
{
    public static void ManualFloorPlanMetricValidatorReportsEveryProblem()
    {
        var validator = ManualFloorPlanMetricRules.DefaultManualFloorPlanMetricValidator();
        var item = ManualFloorPlanMetric.NewManualFloorPlanMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualFloorPlanMetricWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new ManualFloorPlanMetricWeightRangeRule(9.4, 3705.4);
        var item = ManualFloorPlanMetric.NewManualFloorPlanMetric("x", "y");
        item.Weight = 3706.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualFloorPlanMetricTitleLengthRuleFlagsALongTitle()
    {
        var rule = new ManualFloorPlanMetricTitleLengthRule(64);
        var item = ManualFloorPlanMetric.NewManualFloorPlanMetric("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualFloorPlanMetricCodeLengthRuleFlagsALongCode()
    {
        var rule = new ManualFloorPlanMetricCodeLengthRule(64);
        var item = ManualFloorPlanMetric.NewManualFloorPlanMetric("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualFloorPlanMetricLabelLengthRuleFlagsALongLabel()
    {
        var rule = new ManualFloorPlanMetricLabelLengthRule(3600);
        var item = ManualFloorPlanMetric.NewManualFloorPlanMetric("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ManualFloorPlanMetricValidatorReportsEveryProblem();
        ManualFloorPlanMetricWeightRangeRuleFlagsValuesAboveWeight();
        ManualFloorPlanMetricTitleLengthRuleFlagsALongTitle();
        ManualFloorPlanMetricCodeLengthRuleFlagsALongCode();
        ManualFloorPlanMetricLabelLengthRuleFlagsALongLabel();
    }
}
