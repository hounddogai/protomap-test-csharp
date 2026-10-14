namespace Acme.Workforce.Compensation.Tests.PayBand.Snapshot;

using Acme.Workforce.Compensation.PayBand.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class FallbackPayBandSnapshotViewChecks
{
    public static void FallbackPayBandSnapshotViewShowsTheStatusInCapitals()
    {
        var item = FallbackPayBandSnapshot.NewFallbackPayBandSnapshot("code-520", "title-3");
        var view = FallbackPayBandSnapshotView.ToFallbackPayBandSnapshotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackPayBandSnapshotView.ToFallbackPayBandSnapshotViews(new List<FallbackPayBandSnapshot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackPayBandSnapshotViewShowsTheStatusInCapitals();
    }
}
