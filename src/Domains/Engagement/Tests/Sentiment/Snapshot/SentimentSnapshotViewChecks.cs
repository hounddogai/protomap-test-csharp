namespace Acme.Workforce.Engagement.Tests.Sentiment.Snapshot;

using Acme.Workforce.Engagement.Sentiment.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class SentimentSnapshotViewChecks
{
    public static void SentimentSnapshotViewShowsTheStatusInCapitals()
    {
        var item = SentimentSnapshot.NewSentimentSnapshot("code-274", "title-498");
        var view = SentimentSnapshotView.ToSentimentSnapshotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SentimentSnapshotView.ToSentimentSnapshotViews(new List<SentimentSnapshot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SentimentSnapshotViewShowsTheStatusInCapitals();
    }
}
