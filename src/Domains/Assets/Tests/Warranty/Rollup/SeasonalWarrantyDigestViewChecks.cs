namespace Acme.Workforce.Assets.Tests.Warranty.Rollup;

using Acme.Workforce.Assets.Warranty.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class SeasonalWarrantyDigestViewChecks
{
    public static void SeasonalWarrantyDigestViewShowsTheStatusInCapitals()
    {
        var item = SeasonalWarrantyDigest.NewSeasonalWarrantyDigest("code-946", "title-206");
        var view = SeasonalWarrantyDigestView.ToSeasonalWarrantyDigestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalWarrantyDigestView.ToSeasonalWarrantyDigestViews(new List<SeasonalWarrantyDigest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalWarrantyDigestViewShowsTheStatusInCapitals();
    }
}
