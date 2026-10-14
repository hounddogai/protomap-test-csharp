namespace Acme.Workforce.Workplace.Tests.Occupancy.Plan;

using Acme.Workforce.Workplace.Occupancy.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class ScheduledOccupancyPlanChecks
{
    public static void ScheduledOccupancyPlanStartsInOpen()
    {
        var item = ScheduledOccupancyPlan.NewScheduledOccupancyPlan("code-675", "name-144");
        Check.Equal(item.Status, ScheduledOccupancyPlanStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledOccupancyPlanMovesToInReview()
    {
        var item = ScheduledOccupancyPlan.NewScheduledOccupancyPlan("code-675", "name-144");
        try
        {
            item.MoveTo(ScheduledOccupancyPlanStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledOccupancyPlanStatus.InReview, "status");
    }

    public static void ScheduledOccupancyPlanRejectsMovesFromArchived()
    {
        var item = ScheduledOccupancyPlan.NewScheduledOccupancyPlan("code-675", "name-144");
        item.Status = ScheduledOccupancyPlanStatus.Archived;
        Check.Fails(() => item.MoveTo(ScheduledOccupancyPlanStatus.Open), "move");
    }

    public static void RunAll()
    {
        ScheduledOccupancyPlanStartsInOpen();
        ScheduledOccupancyPlanMovesToInReview();
        ScheduledOccupancyPlanRejectsMovesFromArchived();
    }
}
