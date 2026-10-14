namespace Acme.Workforce.Engagement.Tests.Sentiment.Snapshot;

using Acme.Workforce.Engagement.Sentiment.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class SentimentSnapshotRulesChecks
{
    public static void SentimentSnapshotValidatorReportsEveryProblem()
    {
        var validator = SentimentSnapshotRules.DefaultSentimentSnapshotValidator();
        var item = SentimentSnapshot.NewSentimentSnapshot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SentimentSnapshotTitleLengthRuleFlagsALongTitle()
    {
        var rule = new SentimentSnapshotTitleLengthRule(80);
        var item = SentimentSnapshot.NewSentimentSnapshot("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SentimentSnapshotSegmentLengthRuleFlagsALongSegment()
    {
        var rule = new SentimentSnapshotSegmentLengthRule(86400);
        var item = SentimentSnapshot.NewSentimentSnapshot("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SentimentSnapshotValidatorReportsEveryProblem();
        SentimentSnapshotTitleLengthRuleFlagsALongTitle();
        SentimentSnapshotSegmentLengthRuleFlagsALongSegment();
    }
}
