namespace Acme.Workforce.Orgdesign.Tests.Reorg.Snapshot;

using Acme.Workforce.Orgdesign.Reorg.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class SeasonalReorgSnapshotViewChecks
{
    public static void SeasonalReorgSnapshotViewShowsTheStatusInCapitals()
    {
        var item = SeasonalReorgSnapshot.NewSeasonalReorgSnapshot("code-478", "label-39");
        var view = SeasonalReorgSnapshotView.ToSeasonalReorgSnapshotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalReorgSnapshotView.ToSeasonalReorgSnapshotViews(new List<SeasonalReorgSnapshot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalReorgSnapshotViewShowsTheStatusInCapitals();
    }
}
