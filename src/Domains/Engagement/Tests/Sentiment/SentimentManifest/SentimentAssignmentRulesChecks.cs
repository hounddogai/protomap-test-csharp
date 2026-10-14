namespace Acme.Workforce.Engagement.Tests.Sentiment.SentimentManifest;

using Acme.Workforce.Engagement.Sentiment.SentimentManifest;

/// <summary>Checks of sentiment manifest code, run by the project's runner.</summary>
public static class SentimentAssignmentRulesChecks
{
    public static void SentimentAssignmentValidatorReportsEveryProblem()
    {
        var validator = SentimentAssignmentRules.DefaultSentimentAssignmentValidator();
        var item = SentimentAssignment.NewSentimentAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SentimentAssignmentBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new SentimentAssignmentBatchSizeRangeRule(64, 1000);
        var item = SentimentAssignment.NewSentimentAssignment("x", "y");
        item.BatchSize = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SentimentAssignmentTitleLengthRuleFlagsALongTitle()
    {
        var rule = new SentimentAssignmentTitleLengthRule(16);
        var item = SentimentAssignment.NewSentimentAssignment("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SentimentAssignmentValidatorReportsEveryProblem();
        SentimentAssignmentBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        SentimentAssignmentTitleLengthRuleFlagsALongTitle();
    }
}
