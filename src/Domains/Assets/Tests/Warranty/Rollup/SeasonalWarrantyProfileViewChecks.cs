namespace Acme.Workforce.Assets.Tests.Warranty.Rollup;

using Acme.Workforce.Assets.Warranty.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class SeasonalWarrantyProfileViewChecks
{
    public static void SeasonalWarrantyProfileViewShowsTheStatusInCapitals()
    {
        var item = SeasonalWarrantyProfile.NewSeasonalWarrantyProfile("code-620", "title-115");
        var view = SeasonalWarrantyProfileView.ToSeasonalWarrantyProfileView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalWarrantyProfileView.ToSeasonalWarrantyProfileViews(new List<SeasonalWarrantyProfile> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalWarrantyProfileViewShowsTheStatusInCapitals();
    }
}
