namespace Acme.Workforce.Assets.Tests.Warranty.WarrantyReservation;

using Acme.Workforce.Assets.Warranty.WarrantyReservation;

/// <summary>Checks of warranty reservation code, run by the project's runner.</summary>
public static class WarrantyBatchViewChecks
{
    public static void WarrantyBatchViewShowsTheStatusInCapitals()
    {
        var item = WarrantyBatch.NewWarrantyBatch("code-422", "name-840");
        var view = WarrantyBatchView.ToWarrantyBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WarrantyBatchView.ToWarrantyBatchViews(new List<WarrantyBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WarrantyBatchViewShowsTheStatusInCapitals();
    }
}
