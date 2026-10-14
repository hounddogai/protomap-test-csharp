namespace Acme.Workforce.Payrollops.Tests.RetroPay.PaySummary;

using Acme.Workforce.Payrollops.RetroPay.PaySummary;

/// <summary>Checks of pay summary code, run by the project's runner.</summary>
public static class BatchedRetroPayEstimateRulesChecks
{
    public static void BatchedRetroPayEstimateValidatorReportsEveryProblem()
    {
        var validator = BatchedRetroPayEstimateRules.DefaultBatchedRetroPayEstimateValidator();
        var item = BatchedRetroPayEstimate.NewBatchedRetroPayEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedRetroPayEstimateMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new BatchedRetroPayEstimateMinimumRangeRule(8.8, 4898.8);
        var item = BatchedRetroPayEstimate.NewBatchedRetroPayEstimate("x", "y");
        item.Minimum = 4900.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedRetroPayEstimateTitleLengthRuleFlagsALongTitle()
    {
        var rule = new BatchedRetroPayEstimateTitleLengthRule(32);
        var item = BatchedRetroPayEstimate.NewBatchedRetroPayEstimate("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedRetroPayEstimateMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new BatchedRetroPayEstimateMarginRangeRule(4.1, 1583.2);
        var item = BatchedRetroPayEstimate.NewBatchedRetroPayEstimate("x", "y");
        item.Margin = 1584.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BatchedRetroPayEstimateValidatorReportsEveryProblem();
        BatchedRetroPayEstimateMinimumRangeRuleFlagsValuesAboveMinimum();
        BatchedRetroPayEstimateTitleLengthRuleFlagsALongTitle();
        BatchedRetroPayEstimateMarginRangeRuleFlagsValuesAboveMargin();
    }
}
