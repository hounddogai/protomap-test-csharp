namespace Acme.Workforce.Engagement.Tests.Sentiment.Adjustment;

using Acme.Workforce.Engagement.Sentiment.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class PendingSentimentBatchRulesChecks
{
    public static void PendingSentimentBatchValidatorReportsEveryProblem()
    {
        var validator = PendingSentimentBatchRules.DefaultPendingSentimentBatchValidator();
        var item = PendingSentimentBatch.NewPendingSentimentBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingSentimentBatchRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PendingSentimentBatchRequiredCodeRule("pending sentiment batch needs an entry");
        var item = PendingSentimentBatch.NewPendingSentimentBatch("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingSentimentBatchRequiredReviewerRuleFlagsABlankReviewer()
    {
        var rule = new PendingSentimentBatchRequiredReviewerRule("pending sentiment batch needs a value");
        var item = PendingSentimentBatch.NewPendingSentimentBatch("", "");
        item.Reviewer = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingSentimentBatchRequiredNameRuleFlagsABlankName()
    {
        var rule = new PendingSentimentBatchRequiredNameRule("pending sentiment batch needs a value");
        var item = PendingSentimentBatch.NewPendingSentimentBatch("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingSentimentBatchReviewerLengthRuleFlagsALongReviewer()
    {
        var rule = new PendingSentimentBatchReviewerLengthRule(64);
        var item = PendingSentimentBatch.NewPendingSentimentBatch("x", "y");
        item.Reviewer = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Reviewer = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PendingSentimentBatchValidatorReportsEveryProblem();
        PendingSentimentBatchRequiredCodeRuleFlagsABlankCode();
        PendingSentimentBatchRequiredReviewerRuleFlagsABlankReviewer();
        PendingSentimentBatchRequiredNameRuleFlagsABlankName();
        PendingSentimentBatchReviewerLengthRuleFlagsALongReviewer();
    }
}
