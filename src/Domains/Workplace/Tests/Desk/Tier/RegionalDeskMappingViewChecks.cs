namespace Acme.Workforce.Workplace.Tests.Desk.Tier;

using Acme.Workforce.Workplace.Desk.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class RegionalDeskMappingViewChecks
{
    public static void RegionalDeskMappingViewShowsTheStatusInCapitals()
    {
        var item = RegionalDeskMapping.NewRegionalDeskMapping("code-235", "title-958");
        var view = RegionalDeskMappingView.ToRegionalDeskMappingView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalDeskMappingView.ToRegionalDeskMappingViews(new List<RegionalDeskMapping> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalDeskMappingViewShowsTheStatusInCapitals();
    }
}
