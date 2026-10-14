namespace Acme.Workforce.Compensation.Tests.MeritCycle.OverrideItem;

using Acme.Workforce.Compensation.MeritCycle.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class MeritCycleSnapshotViewChecks
{
    public static void MeritCycleSnapshotViewShowsTheStatusInCapitals()
    {
        var item = MeritCycleSnapshot.NewMeritCycleSnapshot("code-799", "label-457");
        var view = MeritCycleSnapshotView.ToMeritCycleSnapshotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(MeritCycleSnapshotView.ToMeritCycleSnapshotViews(new List<MeritCycleSnapshot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        MeritCycleSnapshotViewShowsTheStatusInCapitals();
    }
}
