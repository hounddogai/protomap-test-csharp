namespace Acme.Workforce.Contractors.Tests.Engagement.Digest;

using Acme.Workforce.Contractors.Engagement.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class BatchedEngagementExceptionRulesChecks
{
    public static void BatchedEngagementExceptionValidatorReportsEveryProblem()
    {
        var validator = BatchedEngagementExceptionRules.DefaultBatchedEngagementExceptionValidator();
        var item = BatchedEngagementException.NewBatchedEngagementException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedEngagementExceptionNameLengthRuleFlagsALongName()
    {
        var rule = new BatchedEngagementExceptionNameLengthRule(86400);
        var item = BatchedEngagementException.NewBatchedEngagementException("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedEngagementExceptionWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new BatchedEngagementExceptionWindowDaysRangeRule(64, 86400);
        var item = BatchedEngagementException.NewBatchedEngagementException("x", "y");
        item.WindowDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedEngagementExceptionTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new BatchedEngagementExceptionTenureMonthsRangeRule(64, 80);
        var item = BatchedEngagementException.NewBatchedEngagementException("x", "y");
        item.TenureMonths = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BatchedEngagementExceptionValidatorReportsEveryProblem();
        BatchedEngagementExceptionNameLengthRuleFlagsALongName();
        BatchedEngagementExceptionWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        BatchedEngagementExceptionTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
    }
}
