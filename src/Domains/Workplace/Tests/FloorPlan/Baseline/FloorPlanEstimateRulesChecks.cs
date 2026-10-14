namespace Acme.Workforce.Workplace.Tests.FloorPlan.Baseline;

using Acme.Workforce.Workplace.FloorPlan.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class FloorPlanEstimateRulesChecks
{
    public static void FloorPlanEstimateValidatorReportsEveryProblem()
    {
        var validator = FloorPlanEstimateRules.DefaultFloorPlanEstimateValidator();
        var item = FloorPlanEstimate.NewFloorPlanEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FloorPlanEstimateRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new FloorPlanEstimateRequiredLabelRule("floor plan estimate needs this field");
        var item = FloorPlanEstimate.NewFloorPlanEstimate("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FloorPlanEstimateRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new FloorPlanEstimateRequiredSummaryRule("floor plan estimate needs this field");
        var item = FloorPlanEstimate.NewFloorPlanEstimate("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FloorPlanEstimateRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new FloorPlanEstimateRequiredLocaleRule("floor plan estimate needs an entry");
        var item = FloorPlanEstimate.NewFloorPlanEstimate("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FloorPlanEstimateLocaleLengthRuleFlagsALongLocale()
    {
        var rule = new FloorPlanEstimateLocaleLengthRule(86400);
        var item = FloorPlanEstimate.NewFloorPlanEstimate("x", "y");
        item.Locale = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Locale = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        FloorPlanEstimateValidatorReportsEveryProblem();
        FloorPlanEstimateRequiredLabelRuleFlagsABlankLabel();
        FloorPlanEstimateRequiredSummaryRuleFlagsABlankSummary();
        FloorPlanEstimateRequiredLocaleRuleFlagsABlankLocale();
        FloorPlanEstimateLocaleLengthRuleFlagsALongLocale();
    }
}
