namespace Acme.Workforce.Workplace.Tests.Occupancy.Target;

using Acme.Workforce.Workplace.Occupancy.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class OccupancyTargetChecks
{
    public static void OccupancyTargetStartsInPlanned()
    {
        var item = OccupancyTarget.NewOccupancyTarget("code-990", "name-997");
        Check.Equal(item.Status, OccupancyTargetStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OccupancyTargetMovesToCompleted()
    {
        var item = OccupancyTarget.NewOccupancyTarget("code-990", "name-997");
        try
        {
            item.MoveTo(OccupancyTargetStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OccupancyTargetStatus.Completed, "status");
    }

    public static void OccupancyTargetRejectsMovesFromCancelled()
    {
        var item = OccupancyTarget.NewOccupancyTarget("code-990", "name-997");
        item.Status = OccupancyTargetStatus.Cancelled;
        Check.Fails(() => item.MoveTo(OccupancyTargetStatus.Planned), "move");
    }

    public static void RunAll()
    {
        OccupancyTargetStartsInPlanned();
        OccupancyTargetMovesToCompleted();
        OccupancyTargetRejectsMovesFromCancelled();
    }
}
