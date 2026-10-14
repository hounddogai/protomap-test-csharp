namespace Acme.Workforce.Workplace.Tests.Desk.Snapshot;

using Acme.Workforce.Workplace.Desk.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class FallbackDeskSnapshotViewChecks
{
    public static void FallbackDeskSnapshotViewShowsTheStatusInCapitals()
    {
        var item = FallbackDeskSnapshot.NewFallbackDeskSnapshot("code-285", "title-457");
        var view = FallbackDeskSnapshotView.ToFallbackDeskSnapshotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackDeskSnapshotView.ToFallbackDeskSnapshotViews(new List<FallbackDeskSnapshot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackDeskSnapshotViewShowsTheStatusInCapitals();
    }
}
