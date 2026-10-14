namespace Acme.Workforce.Contractors.Tests.Engagement.EngagementSnapshot;

using Acme.Workforce.Contractors.Engagement.EngagementSnapshot;

/// <summary>Checks of engagement snapshot code, run by the project's runner.</summary>
public static class StrictEngagementSnapshotViewChecks
{
    public static void StrictEngagementSnapshotViewShowsTheStatusInCapitals()
    {
        var item = StrictEngagementSnapshot.NewStrictEngagementSnapshot("code-824", "title-67");
        var view = StrictEngagementSnapshotView.ToStrictEngagementSnapshotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictEngagementSnapshotView.ToStrictEngagementSnapshotViews(new List<StrictEngagementSnapshot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictEngagementSnapshotViewShowsTheStatusInCapitals();
    }
}
