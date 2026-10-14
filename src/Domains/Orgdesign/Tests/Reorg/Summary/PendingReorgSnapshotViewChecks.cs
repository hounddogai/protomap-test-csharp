namespace Acme.Workforce.Orgdesign.Tests.Reorg.Summary;

using Acme.Workforce.Orgdesign.Reorg.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class PendingReorgSnapshotViewChecks
{
    public static void PendingReorgSnapshotViewShowsTheStatusInCapitals()
    {
        var item = PendingReorgSnapshot.NewPendingReorgSnapshot("code-788", "label-433");
        var view = PendingReorgSnapshotView.ToPendingReorgSnapshotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingReorgSnapshotView.ToPendingReorgSnapshotViews(new List<PendingReorgSnapshot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingReorgSnapshotViewShowsTheStatusInCapitals();
    }
}
