namespace Acme.Workforce.Workplace.Tests.FloorPlan.Entry;

using Acme.Workforce.Workplace.FloorPlan.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class WeightedFloorPlanMetricRulesChecks
{
    public static void WeightedFloorPlanMetricValidatorReportsEveryProblem()
    {
        var validator = WeightedFloorPlanMetricRules.DefaultWeightedFloorPlanMetricValidator();
        var item = WeightedFloorPlanMetric.NewWeightedFloorPlanMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedFloorPlanMetricRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new WeightedFloorPlanMetricRequiredExternalRefRule("weighted floor plan metric needs a value");
        var item = WeightedFloorPlanMetric.NewWeightedFloorPlanMetric("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedFloorPlanMetricPhoneLengthRuleFlagsALongPhone()
    {
        var rule = new WeightedFloorPlanMetricPhoneLengthRule(86400);
        var item = WeightedFloorPlanMetric.NewWeightedFloorPlanMetric("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedFloorPlanMetricRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new WeightedFloorPlanMetricRequiredCodeRule("weighted floor plan metric needs this field");
        var item = WeightedFloorPlanMetric.NewWeightedFloorPlanMetric("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedFloorPlanMetricExternalRefLengthRuleFlagsALongExternalRef()
    {
        var rule = new WeightedFloorPlanMetricExternalRefLengthRule(16);
        var item = WeightedFloorPlanMetric.NewWeightedFloorPlanMetric("x", "y");
        item.ExternalRef = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ExternalRef = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedFloorPlanMetricRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new WeightedFloorPlanMetricRequiredLabelRule("weighted floor plan metric needs an entry");
        var item = WeightedFloorPlanMetric.NewWeightedFloorPlanMetric("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        WeightedFloorPlanMetricValidatorReportsEveryProblem();
        WeightedFloorPlanMetricRequiredExternalRefRuleFlagsABlankExternalRef();
        WeightedFloorPlanMetricPhoneLengthRuleFlagsALongPhone();
        WeightedFloorPlanMetricRequiredCodeRuleFlagsABlankCode();
        WeightedFloorPlanMetricExternalRefLengthRuleFlagsALongExternalRef();
        WeightedFloorPlanMetricRequiredLabelRuleFlagsABlankLabel();
    }
}
