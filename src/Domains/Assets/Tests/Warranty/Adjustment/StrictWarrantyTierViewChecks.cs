namespace Acme.Workforce.Assets.Tests.Warranty.Adjustment;

using Acme.Workforce.Assets.Warranty.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class StrictWarrantyTierViewChecks
{
    public static void StrictWarrantyTierViewShowsTheStatusInCapitals()
    {
        var item = StrictWarrantyTier.NewStrictWarrantyTier("code-104", "name-678");
        var view = StrictWarrantyTierView.ToStrictWarrantyTierView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictWarrantyTierView.ToStrictWarrantyTierViews(new List<StrictWarrantyTier> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictWarrantyTierViewShowsTheStatusInCapitals();
    }
}
