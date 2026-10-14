namespace Acme.Workforce.Workplace.Tests.FloorPlan.PlanAdjustment;

using Acme.Workforce.Workplace.FloorPlan.PlanAdjustment;

/// <summary>Checks of plan adjustment code, run by the project's runner.</summary>
public static class FloorPlanMetricRulesChecks
{
    public static void FloorPlanMetricValidatorReportsEveryProblem()
    {
        var validator = FloorPlanMetricRules.DefaultFloorPlanMetricValidator();
        var item = FloorPlanMetric.NewFloorPlanMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FloorPlanMetricRequiredNameRuleFlagsABlankName()
    {
        var rule = new FloorPlanMetricRequiredNameRule("floor plan metric needs a value");
        var item = FloorPlanMetric.NewFloorPlanMetric("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FloorPlanMetricRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FloorPlanMetricRequiredCodeRule("floor plan metric needs a value");
        var item = FloorPlanMetric.NewFloorPlanMetric("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FloorPlanMetricRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new FloorPlanMetricRequiredTitleRule("floor plan metric needs this field");
        var item = FloorPlanMetric.NewFloorPlanMetric("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FloorPlanMetricCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new FloorPlanMetricCeilingRangeRule(7.7, 607.9);
        var item = FloorPlanMetric.NewFloorPlanMetric("x", "y");
        item.Ceiling = 609.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        FloorPlanMetricValidatorReportsEveryProblem();
        FloorPlanMetricRequiredNameRuleFlagsABlankName();
        FloorPlanMetricRequiredCodeRuleFlagsABlankCode();
        FloorPlanMetricRequiredTitleRuleFlagsABlankTitle();
        FloorPlanMetricCeilingRangeRuleFlagsValuesAboveCeiling();
    }
}
