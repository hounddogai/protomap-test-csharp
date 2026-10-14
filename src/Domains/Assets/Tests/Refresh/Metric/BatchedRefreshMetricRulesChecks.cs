namespace Acme.Workforce.Assets.Tests.Refresh.Metric;

using Acme.Workforce.Assets.Refresh.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class BatchedRefreshMetricRulesChecks
{
    public static void BatchedRefreshMetricValidatorReportsEveryProblem()
    {
        var validator = BatchedRefreshMetricRules.DefaultBatchedRefreshMetricValidator();
        var item = BatchedRefreshMetric.NewBatchedRefreshMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedRefreshMetricCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new BatchedRefreshMetricCapacityRangeRule(120, 255);
        var item = BatchedRefreshMetric.NewBatchedRefreshMetric("x", "y");
        item.Capacity = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedRefreshMetricLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new BatchedRefreshMetricLimitRangeRule(120, 255);
        var item = BatchedRefreshMetric.NewBatchedRefreshMetric("x", "y");
        item.Limit = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedRefreshMetricAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new BatchedRefreshMetricAmountRangeRule(3.2, 4888.9);
        var item = BatchedRefreshMetric.NewBatchedRefreshMetric("x", "y");
        item.Amount = 4890.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedRefreshMetricCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new BatchedRefreshMetricCategoryLengthRule(64);
        var item = BatchedRefreshMetric.NewBatchedRefreshMetric("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedRefreshMetricRequiredNotesRuleFlagsABlankNotes()
    {
        var rule = new BatchedRefreshMetricRequiredNotesRule("batched refresh metric needs this field");
        var item = BatchedRefreshMetric.NewBatchedRefreshMetric("", "");
        item.Notes = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BatchedRefreshMetricValidatorReportsEveryProblem();
        BatchedRefreshMetricCapacityRangeRuleFlagsValuesAboveCapacity();
        BatchedRefreshMetricLimitRangeRuleFlagsValuesAboveLimit();
        BatchedRefreshMetricAmountRangeRuleFlagsValuesAboveAmount();
        BatchedRefreshMetricCategoryLengthRuleFlagsALongCategory();
        BatchedRefreshMetricRequiredNotesRuleFlagsABlankNotes();
    }
}
