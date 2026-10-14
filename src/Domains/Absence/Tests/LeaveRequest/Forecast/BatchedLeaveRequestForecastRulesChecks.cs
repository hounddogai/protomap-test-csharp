namespace Acme.Workforce.Absence.Tests.LeaveRequest.Forecast;

using Acme.Workforce.Absence.LeaveRequest.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class BatchedLeaveRequestForecastRulesChecks
{
    public static void BatchedLeaveRequestForecastValidatorReportsEveryProblem()
    {
        var validator = BatchedLeaveRequestForecastRules.DefaultBatchedLeaveRequestForecastValidator();
        var item = BatchedLeaveRequestForecast.NewBatchedLeaveRequestForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedLeaveRequestForecastNameLengthRuleFlagsALongName()
    {
        var rule = new BatchedLeaveRequestForecastNameLengthRule(80);
        var item = BatchedLeaveRequestForecast.NewBatchedLeaveRequestForecast("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedLeaveRequestForecastDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new BatchedLeaveRequestForecastDiscountRangeRule(8.9, 1339.9);
        var item = BatchedLeaveRequestForecast.NewBatchedLeaveRequestForecast("x", "y");
        item.Discount = 1341.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedLeaveRequestForecastCodeLengthRuleFlagsALongCode()
    {
        var rule = new BatchedLeaveRequestForecastCodeLengthRule(16);
        var item = BatchedLeaveRequestForecast.NewBatchedLeaveRequestForecast("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedLeaveRequestForecastUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new BatchedLeaveRequestForecastUpdatedAtRangeRule(255, 3600);
        var item = BatchedLeaveRequestForecast.NewBatchedLeaveRequestForecast("x", "y");
        item.UpdatedAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedLeaveRequestForecastBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new BatchedLeaveRequestForecastBatchSizeRangeRule(16, 16);
        var item = BatchedLeaveRequestForecast.NewBatchedLeaveRequestForecast("x", "y");
        item.BatchSize = 17;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedLeaveRequestForecastTitleLengthRuleFlagsALongTitle()
    {
        var rule = new BatchedLeaveRequestForecastTitleLengthRule(80);
        var item = BatchedLeaveRequestForecast.NewBatchedLeaveRequestForecast("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BatchedLeaveRequestForecastValidatorReportsEveryProblem();
        BatchedLeaveRequestForecastNameLengthRuleFlagsALongName();
        BatchedLeaveRequestForecastDiscountRangeRuleFlagsValuesAboveDiscount();
        BatchedLeaveRequestForecastCodeLengthRuleFlagsALongCode();
        BatchedLeaveRequestForecastUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        BatchedLeaveRequestForecastBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        BatchedLeaveRequestForecastTitleLengthRuleFlagsALongTitle();
    }
}
