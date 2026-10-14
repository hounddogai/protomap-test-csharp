namespace Acme.Workforce.Payrollops.Tests.Garnishment.Allocation;

using Acme.Workforce.Payrollops.Garnishment.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class BatchedGarnishmentMetricRulesChecks
{
    public static void BatchedGarnishmentMetricValidatorReportsEveryProblem()
    {
        var validator = BatchedGarnishmentMetricRules.DefaultBatchedGarnishmentMetricValidator();
        var item = BatchedGarnishmentMetric.NewBatchedGarnishmentMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedGarnishmentMetricRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new BatchedGarnishmentMetricRetriesRangeRule(16, 3600);
        var item = BatchedGarnishmentMetric.NewBatchedGarnishmentMetric("x", "y");
        item.Retries = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedGarnishmentMetricHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new BatchedGarnishmentMetricHoursRangeRule(2.4, 4517.1);
        var item = BatchedGarnishmentMetric.NewBatchedGarnishmentMetric("x", "y");
        item.Hours = 4518.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedGarnishmentMetricNameLengthRuleFlagsALongName()
    {
        var rule = new BatchedGarnishmentMetricNameLengthRule(1000);
        var item = BatchedGarnishmentMetric.NewBatchedGarnishmentMetric("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedGarnishmentMetricRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new BatchedGarnishmentMetricRequiredContactEmailRule("batched garnishment metric needs a value");
        var item = BatchedGarnishmentMetric.NewBatchedGarnishmentMetric("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedGarnishmentMetricRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BatchedGarnishmentMetricRequiredCodeRule("batched garnishment metric needs a value");
        var item = BatchedGarnishmentMetric.NewBatchedGarnishmentMetric("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BatchedGarnishmentMetricValidatorReportsEveryProblem();
        BatchedGarnishmentMetricRetriesRangeRuleFlagsValuesAboveRetries();
        BatchedGarnishmentMetricHoursRangeRuleFlagsValuesAboveHours();
        BatchedGarnishmentMetricNameLengthRuleFlagsALongName();
        BatchedGarnishmentMetricRequiredContactEmailRuleFlagsABlankContactEmail();
        BatchedGarnishmentMetricRequiredCodeRuleFlagsABlankCode();
    }
}
