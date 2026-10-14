namespace Acme.Workforce.Talent.Tests.Pool.PoolDraft;

using Acme.Workforce.Talent.Pool.PoolDraft;

/// <summary>Checks of pool draft code, run by the project's runner.</summary>
public static class SeasonalPoolOverrideViewChecks
{
    public static void SeasonalPoolOverrideViewShowsTheStatusInCapitals()
    {
        var item = SeasonalPoolOverride.NewSeasonalPoolOverride("code-230", "title-903");
        var view = SeasonalPoolOverrideView.ToSeasonalPoolOverrideView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalPoolOverrideView.ToSeasonalPoolOverrideViews(new List<SeasonalPoolOverride> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalPoolOverrideViewShowsTheStatusInCapitals();
    }
}
