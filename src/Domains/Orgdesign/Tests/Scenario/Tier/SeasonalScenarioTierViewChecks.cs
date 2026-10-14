namespace Acme.Workforce.Orgdesign.Tests.Scenario.Tier;

using Acme.Workforce.Orgdesign.Scenario.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class SeasonalScenarioTierViewChecks
{
    public static void SeasonalScenarioTierViewShowsTheStatusInCapitals()
    {
        var item = SeasonalScenarioTier.NewSeasonalScenarioTier("code-502", "name-356");
        var view = SeasonalScenarioTierView.ToSeasonalScenarioTierView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalScenarioTierView.ToSeasonalScenarioTierViews(new List<SeasonalScenarioTier> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalScenarioTierViewShowsTheStatusInCapitals();
    }
}
