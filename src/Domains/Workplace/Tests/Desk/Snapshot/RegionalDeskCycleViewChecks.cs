namespace Acme.Workforce.Workplace.Tests.Desk.Snapshot;

using Acme.Workforce.Workplace.Desk.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class RegionalDeskCycleViewChecks
{
    public static void RegionalDeskCycleViewShowsTheStatusInCapitals()
    {
        var item = RegionalDeskCycle.NewRegionalDeskCycle("code-763", "name-503");
        var view = RegionalDeskCycleView.ToRegionalDeskCycleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalDeskCycleView.ToRegionalDeskCycleViews(new List<RegionalDeskCycle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalDeskCycleViewShowsTheStatusInCapitals();
    }
}
