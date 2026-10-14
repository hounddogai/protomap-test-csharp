namespace Acme.Workforce.Compensation.Tests.MeritCycle.Mapping;

using Acme.Workforce.Compensation.MeritCycle.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class ScheduledMeritCycleSnapshotViewChecks
{
    public static void ScheduledMeritCycleSnapshotViewShowsTheStatusInCapitals()
    {
        var item = ScheduledMeritCycleSnapshot.NewScheduledMeritCycleSnapshot("code-872", "name-822");
        var view = ScheduledMeritCycleSnapshotView.ToScheduledMeritCycleSnapshotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledMeritCycleSnapshotView.ToScheduledMeritCycleSnapshotViews(new List<ScheduledMeritCycleSnapshot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledMeritCycleSnapshotViewShowsTheStatusInCapitals();
    }
}
