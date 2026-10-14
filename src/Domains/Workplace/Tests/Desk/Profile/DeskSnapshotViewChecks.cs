namespace Acme.Workforce.Workplace.Tests.Desk.Profile;

using Acme.Workforce.Workplace.Desk.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class DeskSnapshotViewChecks
{
    public static void DeskSnapshotViewShowsTheStatusInCapitals()
    {
        var item = DeskSnapshot.NewDeskSnapshot("code-931", "label-799");
        var view = DeskSnapshotView.ToDeskSnapshotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DeskSnapshotView.ToDeskSnapshotViews(new List<DeskSnapshot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DeskSnapshotViewShowsTheStatusInCapitals();
    }
}
