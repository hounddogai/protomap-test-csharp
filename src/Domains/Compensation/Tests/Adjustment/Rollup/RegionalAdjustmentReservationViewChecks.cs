namespace Acme.Workforce.Compensation.Tests.Adjustment.Rollup;

using Acme.Workforce.Compensation.Adjustment.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class RegionalAdjustmentReservationViewChecks
{
    public static void RegionalAdjustmentReservationViewShowsTheStatusInCapitals()
    {
        var item = RegionalAdjustmentReservation.NewRegionalAdjustmentReservation("code-85", "title-363");
        var view = RegionalAdjustmentReservationView.ToRegionalAdjustmentReservationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalAdjustmentReservationView.ToRegionalAdjustmentReservationViews(new List<RegionalAdjustmentReservation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalAdjustmentReservationViewShowsTheStatusInCapitals();
    }
}
