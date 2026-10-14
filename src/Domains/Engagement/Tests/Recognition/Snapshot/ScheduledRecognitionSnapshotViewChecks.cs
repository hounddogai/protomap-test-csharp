namespace Acme.Workforce.Engagement.Tests.Recognition.Snapshot;

using Acme.Workforce.Engagement.Recognition.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class ScheduledRecognitionSnapshotViewChecks
{
    public static void ScheduledRecognitionSnapshotViewShowsTheStatusInCapitals()
    {
        var item = ScheduledRecognitionSnapshot.NewScheduledRecognitionSnapshot("code-862", "title-413");
        var view = ScheduledRecognitionSnapshotView.ToScheduledRecognitionSnapshotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledRecognitionSnapshotView.ToScheduledRecognitionSnapshotViews(new List<ScheduledRecognitionSnapshot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledRecognitionSnapshotViewShowsTheStatusInCapitals();
    }
}
