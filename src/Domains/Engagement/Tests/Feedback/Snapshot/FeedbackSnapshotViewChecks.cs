namespace Acme.Workforce.Engagement.Tests.Feedback.Snapshot;

using Acme.Workforce.Engagement.Feedback.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class FeedbackSnapshotViewChecks
{
    public static void FeedbackSnapshotViewShowsTheStatusInCapitals()
    {
        var item = FeedbackSnapshot.NewFeedbackSnapshot("code-629", "label-43");
        var view = FeedbackSnapshotView.ToFeedbackSnapshotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FeedbackSnapshotView.ToFeedbackSnapshotViews(new List<FeedbackSnapshot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FeedbackSnapshotViewShowsTheStatusInCapitals();
    }
}
