namespace Acme.Workforce.Workplace.Tests.Occupancy.Summary;

using Acme.Workforce.Workplace.Occupancy.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class OccupancyPlanChecks
{
    public static void OccupancyPlanStartsInOpen()
    {
        var item = OccupancyPlan.NewOccupancyPlan("code-619", "name-855");
        Check.Equal(item.Status, OccupancyPlanStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OccupancyPlanMovesToArchived()
    {
        var item = OccupancyPlan.NewOccupancyPlan("code-619", "name-855");
        try
        {
            item.MoveTo(OccupancyPlanStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OccupancyPlanStatus.Archived, "status");
    }

    public static void OccupancyPlanRejectsMovesFromArchived()
    {
        var item = OccupancyPlan.NewOccupancyPlan("code-619", "name-855");
        item.Status = OccupancyPlanStatus.Archived;
        Check.Fails(() => item.MoveTo(OccupancyPlanStatus.Open), "move");
    }

    public static void RunAll()
    {
        OccupancyPlanStartsInOpen();
        OccupancyPlanMovesToArchived();
        OccupancyPlanRejectsMovesFromArchived();
    }
}
