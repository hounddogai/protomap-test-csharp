namespace Acme.Workforce.Workplace.Tests.Occupancy.OccupancyPlan;

using Acme.Workforce.Workplace.Occupancy.OccupancyPlan;

/// <summary>Checks of occupancy plan code, run by the project's runner.</summary>
public static class OccupancyAssignmentViewChecks
{
    public static void OccupancyAssignmentViewShowsTheStatusInCapitals()
    {
        var item = OccupancyAssignment.NewOccupancyAssignment("code-790", "name-645");
        var view = OccupancyAssignmentView.ToOccupancyAssignmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(OccupancyAssignmentView.ToOccupancyAssignmentViews(new List<OccupancyAssignment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        OccupancyAssignmentViewShowsTheStatusInCapitals();
    }
}
