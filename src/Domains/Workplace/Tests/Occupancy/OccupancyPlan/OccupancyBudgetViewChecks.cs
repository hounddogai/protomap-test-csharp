namespace Acme.Workforce.Workplace.Tests.Occupancy.OccupancyPlan;

using Acme.Workforce.Workplace.Occupancy.OccupancyPlan;

/// <summary>Checks of occupancy plan code, run by the project's runner.</summary>
public static class OccupancyBudgetViewChecks
{
    public static void OccupancyBudgetViewShowsTheStatusInCapitals()
    {
        var item = OccupancyBudget.NewOccupancyBudget("code-237", "label-556");
        var view = OccupancyBudgetView.ToOccupancyBudgetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(OccupancyBudgetView.ToOccupancyBudgetViews(new List<OccupancyBudget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        OccupancyBudgetViewShowsTheStatusInCapitals();
    }
}
