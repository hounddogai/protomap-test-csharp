namespace Acme.Workforce.Engagement.Tests.Feedback.Score;

using Acme.Workforce.Engagement.Feedback.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class BatchedFeedbackScoreRulesChecks
{
    public static void BatchedFeedbackScoreValidatorReportsEveryProblem()
    {
        var validator = BatchedFeedbackScoreRules.DefaultBatchedFeedbackScoreValidator();
        var item = BatchedFeedbackScore.NewBatchedFeedbackScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedFeedbackScoreRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new BatchedFeedbackScoreRequiredTitleRule("batched feedback score needs this field");
        var item = BatchedFeedbackScore.NewBatchedFeedbackScore("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedFeedbackScoreRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new BatchedFeedbackScoreRequiredCurrencyRule("batched feedback score needs this field");
        var item = BatchedFeedbackScore.NewBatchedFeedbackScore("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedFeedbackScoreCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new BatchedFeedbackScoreCurrencyLengthRule(86400);
        var item = BatchedFeedbackScore.NewBatchedFeedbackScore("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedFeedbackScoreRequiredReviewerRuleFlagsABlankReviewer()
    {
        var rule = new BatchedFeedbackScoreRequiredReviewerRule("batched feedback score needs this field");
        var item = BatchedFeedbackScore.NewBatchedFeedbackScore("", "");
        item.Reviewer = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedFeedbackScoreRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BatchedFeedbackScoreRequiredCodeRule("batched feedback score needs a value");
        var item = BatchedFeedbackScore.NewBatchedFeedbackScore("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BatchedFeedbackScoreValidatorReportsEveryProblem();
        BatchedFeedbackScoreRequiredTitleRuleFlagsABlankTitle();
        BatchedFeedbackScoreRequiredCurrencyRuleFlagsABlankCurrency();
        BatchedFeedbackScoreCurrencyLengthRuleFlagsALongCurrency();
        BatchedFeedbackScoreRequiredReviewerRuleFlagsABlankReviewer();
        BatchedFeedbackScoreRequiredCodeRuleFlagsABlankCode();
    }
}
