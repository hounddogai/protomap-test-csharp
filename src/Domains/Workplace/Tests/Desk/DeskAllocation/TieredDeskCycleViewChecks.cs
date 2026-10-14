namespace Acme.Workforce.Workplace.Tests.Desk.DeskAllocation;

using Acme.Workforce.Workplace.Desk.DeskAllocation;

/// <summary>Checks of desk allocation code, run by the project's runner.</summary>
public static class TieredDeskCycleViewChecks
{
    public static void TieredDeskCycleViewShowsTheStatusInCapitals()
    {
        var item = TieredDeskCycle.NewTieredDeskCycle("code-234", "label-385");
        var view = TieredDeskCycleView.ToTieredDeskCycleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredDeskCycleView.ToTieredDeskCycleViews(new List<TieredDeskCycle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredDeskCycleViewShowsTheStatusInCapitals();
    }
}
