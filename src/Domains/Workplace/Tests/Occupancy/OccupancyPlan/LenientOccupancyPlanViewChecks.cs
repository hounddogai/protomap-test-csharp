namespace Acme.Workforce.Workplace.Tests.Occupancy.OccupancyPlan;

using Acme.Workforce.Workplace.Occupancy.OccupancyPlan;

/// <summary>Checks of occupancy plan code, run by the project's runner.</summary>
public static class LenientOccupancyPlanViewChecks
{
    public static void LenientOccupancyPlanViewShowsTheStatusInCapitals()
    {
        var item = LenientOccupancyPlan.NewLenientOccupancyPlan("code-479", "title-745");
        var view = LenientOccupancyPlanView.ToLenientOccupancyPlanView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientOccupancyPlanView.ToLenientOccupancyPlanViews(new List<LenientOccupancyPlan> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientOccupancyPlanViewShowsTheStatusInCapitals();
    }
}
