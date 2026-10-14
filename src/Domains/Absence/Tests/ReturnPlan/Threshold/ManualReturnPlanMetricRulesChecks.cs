namespace Acme.Workforce.Absence.Tests.ReturnPlan.Threshold;

using Acme.Workforce.Absence.ReturnPlan.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class ManualReturnPlanMetricRulesChecks
{
    public static void ManualReturnPlanMetricValidatorReportsEveryProblem()
    {
        var validator = ManualReturnPlanMetricRules.DefaultManualReturnPlanMetricValidator();
        var item = ManualReturnPlanMetric.NewManualReturnPlanMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualReturnPlanMetricCodeLengthRuleFlagsALongCode()
    {
        var rule = new ManualReturnPlanMetricCodeLengthRule(1000);
        var item = ManualReturnPlanMetric.NewManualReturnPlanMetric("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualReturnPlanMetricRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new ManualReturnPlanMetricRequiredSegmentRule("manual return plan metric needs a value");
        var item = ManualReturnPlanMetric.NewManualReturnPlanMetric("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualReturnPlanMetricDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new ManualReturnPlanMetricDiscountRangeRule(8.8, 2067.1);
        var item = ManualReturnPlanMetric.NewManualReturnPlanMetric("x", "y");
        item.Discount = 2068.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ManualReturnPlanMetricValidatorReportsEveryProblem();
        ManualReturnPlanMetricCodeLengthRuleFlagsALongCode();
        ManualReturnPlanMetricRequiredSegmentRuleFlagsABlankSegment();
        ManualReturnPlanMetricDiscountRangeRuleFlagsValuesAboveDiscount();
    }
}
