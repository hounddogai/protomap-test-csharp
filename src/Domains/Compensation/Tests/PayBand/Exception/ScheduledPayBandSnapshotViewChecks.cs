namespace Acme.Workforce.Compensation.Tests.PayBand.Exception;

using Acme.Workforce.Compensation.PayBand.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class ScheduledPayBandSnapshotViewChecks
{
    public static void ScheduledPayBandSnapshotViewShowsTheStatusInCapitals()
    {
        var item = ScheduledPayBandSnapshot.NewScheduledPayBandSnapshot("code-547", "name-663");
        var view = ScheduledPayBandSnapshotView.ToScheduledPayBandSnapshotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledPayBandSnapshotView.ToScheduledPayBandSnapshotViews(new List<ScheduledPayBandSnapshot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledPayBandSnapshotViewShowsTheStatusInCapitals();
    }
}
