namespace Acme.Workforce.Workplace.Tests.Occupancy.Plan;

using Acme.Workforce.Workplace.Occupancy.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class ScheduledOccupancyPlanViewChecks
{
    public static void ScheduledOccupancyPlanViewShowsTheStatusInCapitals()
    {
        var item = ScheduledOccupancyPlan.NewScheduledOccupancyPlan("code-675", "name-144");
        var view = ScheduledOccupancyPlanView.ToScheduledOccupancyPlanView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledOccupancyPlanView.ToScheduledOccupancyPlanViews(new List<ScheduledOccupancyPlan> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledOccupancyPlanViewShowsTheStatusInCapitals();
    }
}
