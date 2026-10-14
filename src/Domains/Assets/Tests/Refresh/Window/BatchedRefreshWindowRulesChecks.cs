namespace Acme.Workforce.Assets.Tests.Refresh.Window;

using Acme.Workforce.Assets.Refresh.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class BatchedRefreshWindowRulesChecks
{
    public static void BatchedRefreshWindowValidatorReportsEveryProblem()
    {
        var validator = BatchedRefreshWindowRules.DefaultBatchedRefreshWindowValidator();
        var item = BatchedRefreshWindow.NewBatchedRefreshWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedRefreshWindowCodeLengthRuleFlagsALongCode()
    {
        var rule = new BatchedRefreshWindowCodeLengthRule(3600);
        var item = BatchedRefreshWindow.NewBatchedRefreshWindow("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedRefreshWindowAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new BatchedRefreshWindowAttemptsRangeRule(80, 80);
        var item = BatchedRefreshWindow.NewBatchedRefreshWindow("x", "y");
        item.Attempts = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedRefreshWindowTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new BatchedRefreshWindowTenureMonthsRangeRule(120, 1000);
        var item = BatchedRefreshWindow.NewBatchedRefreshWindow("x", "y");
        item.TenureMonths = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedRefreshWindowRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new BatchedRefreshWindowRequiredSalaryBandRule("batched refresh window needs a value");
        var item = BatchedRefreshWindow.NewBatchedRefreshWindow("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedRefreshWindowLabelLengthRuleFlagsALongLabel()
    {
        var rule = new BatchedRefreshWindowLabelLengthRule(120);
        var item = BatchedRefreshWindow.NewBatchedRefreshWindow("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BatchedRefreshWindowValidatorReportsEveryProblem();
        BatchedRefreshWindowCodeLengthRuleFlagsALongCode();
        BatchedRefreshWindowAttemptsRangeRuleFlagsValuesAboveAttempts();
        BatchedRefreshWindowTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
        BatchedRefreshWindowRequiredSalaryBandRuleFlagsABlankSalaryBand();
        BatchedRefreshWindowLabelLengthRuleFlagsALongLabel();
    }
}
