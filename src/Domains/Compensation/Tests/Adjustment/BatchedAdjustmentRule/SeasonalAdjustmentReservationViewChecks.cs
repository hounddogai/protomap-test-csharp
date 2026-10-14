namespace Acme.Workforce.Compensation.Tests.Adjustment.BatchedAdjustmentRule;

using Acme.Workforce.Compensation.Adjustment.BatchedAdjustmentRule;

/// <summary>Checks of batched adjustment rule code, run by the project's runner.</summary>
public static class SeasonalAdjustmentReservationViewChecks
{
    public static void SeasonalAdjustmentReservationViewShowsTheStatusInCapitals()
    {
        var item = SeasonalAdjustmentReservation.NewSeasonalAdjustmentReservation("code-664", "label-850");
        var view = SeasonalAdjustmentReservationView.ToSeasonalAdjustmentReservationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalAdjustmentReservationView.ToSeasonalAdjustmentReservationViews(new List<SeasonalAdjustmentReservation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalAdjustmentReservationViewShowsTheStatusInCapitals();
    }
}
