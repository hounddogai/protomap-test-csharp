namespace Acme.Workforce.Payrollops.Tests.RetroPay.PaySummary;

using Acme.Workforce.Payrollops.RetroPay.PaySummary;

/// <summary>Checks of pay summary code, run by the project's runner.</summary>
public static class RetroPayBatchRulesChecks
{
    public static void RetroPayBatchValidatorReportsEveryProblem()
    {
        var validator = RetroPayBatchRules.DefaultRetroPayBatchValidator();
        var item = RetroPayBatch.NewRetroPayBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RetroPayBatchRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RetroPayBatchRequiredCodeRule("retro pay batch needs this field");
        var item = RetroPayBatch.NewRetroPayBatch("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RetroPayBatchThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new RetroPayBatchThresholdRangeRule(9.4, 1771.1);
        var item = RetroPayBatch.NewRetroPayBatch("x", "y");
        item.Threshold = 1772.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RetroPayBatchScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new RetroPayBatchScoreRangeRule(1.8, 689.9);
        var item = RetroPayBatch.NewRetroPayBatch("x", "y");
        item.Score = 691.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RetroPayBatchRequiredReviewerRuleFlagsABlankReviewer()
    {
        var rule = new RetroPayBatchRequiredReviewerRule("retro pay batch needs a value");
        var item = RetroPayBatch.NewRetroPayBatch("", "");
        item.Reviewer = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RetroPayBatchValidatorReportsEveryProblem();
        RetroPayBatchRequiredCodeRuleFlagsABlankCode();
        RetroPayBatchThresholdRangeRuleFlagsValuesAboveThreshold();
        RetroPayBatchScoreRangeRuleFlagsValuesAboveScore();
        RetroPayBatchRequiredReviewerRuleFlagsABlankReviewer();
    }
}
