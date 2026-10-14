namespace Acme.Workforce.Assets.Tests.Warranty.WarrantyReservation;

using Acme.Workforce.Assets.Warranty.WarrantyReservation;

/// <summary>Checks of warranty reservation code, run by the project's runner.</summary>
public static class ManualWarrantyReservationViewChecks
{
    public static void ManualWarrantyReservationViewShowsTheStatusInCapitals()
    {
        var item = ManualWarrantyReservation.NewManualWarrantyReservation("code-276", "label-834");
        var view = ManualWarrantyReservationView.ToManualWarrantyReservationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualWarrantyReservationView.ToManualWarrantyReservationViews(new List<ManualWarrantyReservation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualWarrantyReservationViewShowsTheStatusInCapitals();
    }
}
