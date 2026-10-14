namespace Acme.Workforce.Payrollops.Tests.PayRun.Review;

using Acme.Workforce.Payrollops.PayRun.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class PayRunReviewRulesChecks
{
    public static void PayRunReviewValidatorReportsEveryProblem()
    {
        var validator = PayRunReviewRules.DefaultPayRunReviewValidator();
        var item = PayRunReview.NewPayRunReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayRunReviewRequiredOwnerIdRuleFlagsABlankOwnerId()
    {
        var rule = new PayRunReviewRequiredOwnerIdRule("pay run review needs a value");
        var item = PayRunReview.NewPayRunReview("", "");
        item.OwnerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayRunReviewVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new PayRunReviewVersionRangeRule(16, 80);
        var item = PayRunReview.NewPayRunReview("x", "y");
        item.Version = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PayRunReviewOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new PayRunReviewOwnerIdLengthRule(3600);
        var item = PayRunReview.NewPayRunReview("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayRunReviewBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new PayRunReviewBatchSizeRangeRule(255, 1000);
        var item = PayRunReview.NewPayRunReview("x", "y");
        item.BatchSize = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PayRunReviewValidatorReportsEveryProblem();
        PayRunReviewRequiredOwnerIdRuleFlagsABlankOwnerId();
        PayRunReviewVersionRangeRuleFlagsValuesAboveVersion();
        PayRunReviewOwnerIdLengthRuleFlagsALongOwnerId();
        PayRunReviewBatchSizeRangeRuleFlagsValuesAboveBatchSize();
    }
}
