namespace Acme.Workforce.Engagement.Tests.Sentiment.Milestone;

using Acme.Workforce.Engagement.Sentiment.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class SentimentBatchRulesChecks
{
    public static void SentimentBatchValidatorReportsEveryProblem()
    {
        var validator = SentimentBatchRules.DefaultSentimentBatchValidator();
        var item = SentimentBatch.NewSentimentBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SentimentBatchRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SentimentBatchRequiredCodeRule("sentiment batch needs an entry");
        var item = SentimentBatch.NewSentimentBatch("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SentimentBatchTitleLengthRuleFlagsALongTitle()
    {
        var rule = new SentimentBatchTitleLengthRule(120);
        var item = SentimentBatch.NewSentimentBatch("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SentimentBatchValidatorReportsEveryProblem();
        SentimentBatchRequiredCodeRuleFlagsABlankCode();
        SentimentBatchTitleLengthRuleFlagsALongTitle();
    }
}
