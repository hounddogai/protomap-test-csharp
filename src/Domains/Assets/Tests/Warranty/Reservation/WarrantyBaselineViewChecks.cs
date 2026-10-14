namespace Acme.Workforce.Assets.Tests.Warranty.Reservation;

using Acme.Workforce.Assets.Warranty.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class WarrantyBaselineViewChecks
{
    public static void WarrantyBaselineViewShowsTheStatusInCapitals()
    {
        var item = WarrantyBaseline.NewWarrantyBaseline("code-278", "label-977");
        var view = WarrantyBaselineView.ToWarrantyBaselineView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WarrantyBaselineView.ToWarrantyBaselineViews(new List<WarrantyBaseline> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WarrantyBaselineViewShowsTheStatusInCapitals();
    }
}
