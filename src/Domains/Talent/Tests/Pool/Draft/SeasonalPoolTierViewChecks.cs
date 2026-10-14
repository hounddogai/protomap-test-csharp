namespace Acme.Workforce.Talent.Tests.Pool.Draft;

using Acme.Workforce.Talent.Pool.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class SeasonalPoolTierViewChecks
{
    public static void SeasonalPoolTierViewShowsTheStatusInCapitals()
    {
        var item = SeasonalPoolTier.NewSeasonalPoolTier("code-680", "name-9");
        var view = SeasonalPoolTierView.ToSeasonalPoolTierView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalPoolTierView.ToSeasonalPoolTierViews(new List<SeasonalPoolTier> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalPoolTierViewShowsTheStatusInCapitals();
    }
}
