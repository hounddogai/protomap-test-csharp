namespace Acme.Workforce.Workplace.Tests.Occupancy.Agreement;

using Acme.Workforce.Workplace.Occupancy.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class ManualOccupancyPlanViewChecks
{
    public static void ManualOccupancyPlanViewShowsTheStatusInCapitals()
    {
        var item = ManualOccupancyPlan.NewManualOccupancyPlan("code-863", "name-787");
        var view = ManualOccupancyPlanView.ToManualOccupancyPlanView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualOccupancyPlanView.ToManualOccupancyPlanViews(new List<ManualOccupancyPlan> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualOccupancyPlanViewShowsTheStatusInCapitals();
    }
}
