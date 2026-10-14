namespace Acme.Workforce.Absence.Tests.Holiday.Metric;

using Acme.Workforce.Absence.Holiday.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class TieredHolidaySnapshotViewChecks
{
    public static void TieredHolidaySnapshotViewShowsTheStatusInCapitals()
    {
        var item = TieredHolidaySnapshot.NewTieredHolidaySnapshot("code-378", "name-497");
        var view = TieredHolidaySnapshotView.ToTieredHolidaySnapshotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredHolidaySnapshotView.ToTieredHolidaySnapshotViews(new List<TieredHolidaySnapshot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredHolidaySnapshotViewShowsTheStatusInCapitals();
    }
}
