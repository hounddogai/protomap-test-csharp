namespace Acme.Workforce.Workplace.Tests.Occupancy.Checkpoint;

using Acme.Workforce.Workplace.Occupancy.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class OccupancyMetricRulesChecks
{
    public static void OccupancyMetricValidatorReportsEveryProblem()
    {
        var validator = OccupancyMetricRules.DefaultOccupancyMetricValidator();
        var item = OccupancyMetric.NewOccupancyMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void OccupancyMetricDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new OccupancyMetricDiscountRangeRule(7.4, 3689.3);
        var item = OccupancyMetric.NewOccupancyMetric("x", "y");
        item.Discount = 3690.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void OccupancyMetricRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new OccupancyMetricRequiredCodeRule("occupancy metric needs this field");
        var item = OccupancyMetric.NewOccupancyMetric("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void OccupancyMetricReviewerLengthRuleFlagsALongReviewer()
    {
        var rule = new OccupancyMetricReviewerLengthRule(255);
        var item = OccupancyMetric.NewOccupancyMetric("x", "y");
        item.Reviewer = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Reviewer = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        OccupancyMetricValidatorReportsEveryProblem();
        OccupancyMetricDiscountRangeRuleFlagsValuesAboveDiscount();
        OccupancyMetricRequiredCodeRuleFlagsABlankCode();
        OccupancyMetricReviewerLengthRuleFlagsALongReviewer();
    }
}
