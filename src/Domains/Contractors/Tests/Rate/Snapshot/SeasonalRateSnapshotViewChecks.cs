namespace Acme.Workforce.Contractors.Tests.Rate.Snapshot;

using Acme.Workforce.Contractors.Rate.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class SeasonalRateSnapshotViewChecks
{
    public static void SeasonalRateSnapshotViewShowsTheStatusInCapitals()
    {
        var item = SeasonalRateSnapshot.NewSeasonalRateSnapshot("code-932", "label-16");
        var view = SeasonalRateSnapshotView.ToSeasonalRateSnapshotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalRateSnapshotView.ToSeasonalRateSnapshotViews(new List<SeasonalRateSnapshot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalRateSnapshotViewShowsTheStatusInCapitals();
    }
}
