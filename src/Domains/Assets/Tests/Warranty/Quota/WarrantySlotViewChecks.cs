namespace Acme.Workforce.Assets.Tests.Warranty.Quota;

using Acme.Workforce.Assets.Warranty.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class WarrantySlotViewChecks
{
    public static void WarrantySlotViewShowsTheStatusInCapitals()
    {
        var item = WarrantySlot.NewWarrantySlot("code-33", "label-806");
        var view = WarrantySlotView.ToWarrantySlotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WarrantySlotView.ToWarrantySlotViews(new List<WarrantySlot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WarrantySlotViewShowsTheStatusInCapitals();
    }
}
