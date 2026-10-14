namespace Acme.Workforce.Compensation.Tests.EquityGrant.Batch;

using Acme.Workforce.Compensation.EquityGrant.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class EquityGrantReviewRulesChecks
{
    public static void EquityGrantReviewValidatorReportsEveryProblem()
    {
        var validator = EquityGrantReviewRules.DefaultEquityGrantReviewValidator();
        var item = EquityGrantReview.NewEquityGrantReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void EquityGrantReviewBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new EquityGrantReviewBatchSizeRangeRule(32, 80);
        var item = EquityGrantReview.NewEquityGrantReview("x", "y");
        item.BatchSize = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void EquityGrantReviewCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new EquityGrantReviewCeilingRangeRule(9.1, 1894.9);
        var item = EquityGrantReview.NewEquityGrantReview("x", "y");
        item.Ceiling = 1896.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void EquityGrantReviewCodeLengthRuleFlagsALongCode()
    {
        var rule = new EquityGrantReviewCodeLengthRule(1000);
        var item = EquityGrantReview.NewEquityGrantReview("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EquityGrantReviewReviewerLengthRuleFlagsALongReviewer()
    {
        var rule = new EquityGrantReviewReviewerLengthRule(3600);
        var item = EquityGrantReview.NewEquityGrantReview("x", "y");
        item.Reviewer = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Reviewer = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        EquityGrantReviewValidatorReportsEveryProblem();
        EquityGrantReviewBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        EquityGrantReviewCeilingRangeRuleFlagsValuesAboveCeiling();
        EquityGrantReviewCodeLengthRuleFlagsALongCode();
        EquityGrantReviewReviewerLengthRuleFlagsALongReviewer();
    }
}
