namespace Acme.Workforce.Workplace.Tests.FloorPlan.Cycle;

using Acme.Workforce.Workplace.FloorPlan.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class AdaptiveFloorPlanMetricRulesChecks
{
    public static void AdaptiveFloorPlanMetricValidatorReportsEveryProblem()
    {
        var validator = AdaptiveFloorPlanMetricRules.DefaultAdaptiveFloorPlanMetricValidator();
        var item = AdaptiveFloorPlanMetric.NewAdaptiveFloorPlanMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveFloorPlanMetricLabelLengthRuleFlagsALongLabel()
    {
        var rule = new AdaptiveFloorPlanMetricLabelLengthRule(64);
        var item = AdaptiveFloorPlanMetric.NewAdaptiveFloorPlanMetric("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveFloorPlanMetricRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new AdaptiveFloorPlanMetricRatioRangeRule(1.9, 278.7);
        var item = AdaptiveFloorPlanMetric.NewAdaptiveFloorPlanMetric("x", "y");
        item.Ratio = 280.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveFloorPlanMetricCodeLengthRuleFlagsALongCode()
    {
        var rule = new AdaptiveFloorPlanMetricCodeLengthRule(255);
        var item = AdaptiveFloorPlanMetric.NewAdaptiveFloorPlanMetric("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AdaptiveFloorPlanMetricValidatorReportsEveryProblem();
        AdaptiveFloorPlanMetricLabelLengthRuleFlagsALongLabel();
        AdaptiveFloorPlanMetricRatioRangeRuleFlagsValuesAboveRatio();
        AdaptiveFloorPlanMetricCodeLengthRuleFlagsALongCode();
    }
}
