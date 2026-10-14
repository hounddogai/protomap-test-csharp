namespace Acme.Workforce.Compensation.Tests.Adjustment.Schedule;

using Acme.Workforce.Compensation.Adjustment.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class ScheduledAdjustmentReservationViewChecks
{
    public static void ScheduledAdjustmentReservationViewShowsTheStatusInCapitals()
    {
        var item = ScheduledAdjustmentReservation.NewScheduledAdjustmentReservation("code-651", "title-532");
        var view = ScheduledAdjustmentReservationView.ToScheduledAdjustmentReservationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledAdjustmentReservationView.ToScheduledAdjustmentReservationViews(new List<ScheduledAdjustmentReservation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledAdjustmentReservationViewShowsTheStatusInCapitals();
    }
}
