namespace Acme.Workforce.Assets.Tests.Warranty.WarrantyAllocation;

using Acme.Workforce.Assets.Warranty.WarrantyAllocation;

/// <summary>Checks of warranty allocation code, run by the project's runner.</summary>
public static class SeasonalWarrantyTierViewChecks
{
    public static void SeasonalWarrantyTierViewShowsTheStatusInCapitals()
    {
        var item = SeasonalWarrantyTier.NewSeasonalWarrantyTier("code-492", "name-348");
        var view = SeasonalWarrantyTierView.ToSeasonalWarrantyTierView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalWarrantyTierView.ToSeasonalWarrantyTierViews(new List<SeasonalWarrantyTier> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalWarrantyTierViewShowsTheStatusInCapitals();
    }
}
