namespace Acme.Workforce.Engagement.Tests.TownHall.Ledger;

using Acme.Workforce.Engagement.TownHall.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class RegionalTownHallMappingViewChecks
{
    public static void RegionalTownHallMappingViewShowsTheStatusInCapitals()
    {
        var item = RegionalTownHallMapping.NewRegionalTownHallMapping("code-251", "name-825");
        var view = RegionalTownHallMappingView.ToRegionalTownHallMappingView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalTownHallMappingView.ToRegionalTownHallMappingViews(new List<RegionalTownHallMapping> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalTownHallMappingViewShowsTheStatusInCapitals();
    }
}
