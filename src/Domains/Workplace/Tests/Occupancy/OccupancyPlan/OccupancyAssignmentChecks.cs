namespace Acme.Workforce.Workplace.Tests.Occupancy.OccupancyPlan;

using Acme.Workforce.Workplace.Occupancy.OccupancyPlan;

/// <summary>Checks of occupancy plan code, run by the project's runner.</summary>
public static class OccupancyAssignmentChecks
{
    public static void OccupancyAssignmentStartsInOpen()
    {
        var item = OccupancyAssignment.NewOccupancyAssignment("code-790", "name-645");
        Check.Equal(item.Status, OccupancyAssignmentStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OccupancyAssignmentMovesToResolved()
    {
        var item = OccupancyAssignment.NewOccupancyAssignment("code-790", "name-645");
        try
        {
            item.MoveTo(OccupancyAssignmentStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OccupancyAssignmentStatus.Resolved, "status");
    }

    public static void OccupancyAssignmentRejectsMovesFromArchived()
    {
        var item = OccupancyAssignment.NewOccupancyAssignment("code-790", "name-645");
        item.Status = OccupancyAssignmentStatus.Archived;
        Check.Fails(() => item.MoveTo(OccupancyAssignmentStatus.Open), "move");
    }

    public static void RunAll()
    {
        OccupancyAssignmentStartsInOpen();
        OccupancyAssignmentMovesToResolved();
        OccupancyAssignmentRejectsMovesFromArchived();
    }
}
