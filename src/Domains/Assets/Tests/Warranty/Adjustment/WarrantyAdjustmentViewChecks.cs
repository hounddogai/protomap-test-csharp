namespace Acme.Workforce.Assets.Tests.Warranty.Adjustment;

using Acme.Workforce.Assets.Warranty.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class WarrantyAdjustmentViewChecks
{
    public static void WarrantyAdjustmentViewShowsTheStatusInCapitals()
    {
        var item = WarrantyAdjustment.NewWarrantyAdjustment("code-850", "title-524");
        var view = WarrantyAdjustmentView.ToWarrantyAdjustmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WarrantyAdjustmentView.ToWarrantyAdjustmentViews(new List<WarrantyAdjustment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WarrantyAdjustmentViewShowsTheStatusInCapitals();
    }
}
