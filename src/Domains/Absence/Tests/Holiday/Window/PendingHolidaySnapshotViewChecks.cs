namespace Acme.Workforce.Absence.Tests.Holiday.Window;

using Acme.Workforce.Absence.Holiday.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class PendingHolidaySnapshotViewChecks
{
    public static void PendingHolidaySnapshotViewShowsTheStatusInCapitals()
    {
        var item = PendingHolidaySnapshot.NewPendingHolidaySnapshot("code-378", "name-699");
        var view = PendingHolidaySnapshotView.ToPendingHolidaySnapshotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingHolidaySnapshotView.ToPendingHolidaySnapshotViews(new List<PendingHolidaySnapshot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingHolidaySnapshotViewShowsTheStatusInCapitals();
    }
}
