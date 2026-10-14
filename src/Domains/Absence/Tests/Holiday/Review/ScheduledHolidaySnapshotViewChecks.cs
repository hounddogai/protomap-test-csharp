namespace Acme.Workforce.Absence.Tests.Holiday.Review;

using Acme.Workforce.Absence.Holiday.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class ScheduledHolidaySnapshotViewChecks
{
    public static void ScheduledHolidaySnapshotViewShowsTheStatusInCapitals()
    {
        var item = ScheduledHolidaySnapshot.NewScheduledHolidaySnapshot("code-563", "label-418");
        var view = ScheduledHolidaySnapshotView.ToScheduledHolidaySnapshotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledHolidaySnapshotView.ToScheduledHolidaySnapshotViews(new List<ScheduledHolidaySnapshot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledHolidaySnapshotViewShowsTheStatusInCapitals();
    }
}
