namespace Acme.Workforce.Assets.Tests.Warranty.Reservation;

using Acme.Workforce.Assets.Warranty.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class WarrantyWindowViewChecks
{
    public static void WarrantyWindowViewShowsTheStatusInCapitals()
    {
        var item = WarrantyWindow.NewWarrantyWindow("code-193", "label-795");
        var view = WarrantyWindowView.ToWarrantyWindowView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WarrantyWindowView.ToWarrantyWindowViews(new List<WarrantyWindow> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WarrantyWindowViewShowsTheStatusInCapitals();
    }
}
