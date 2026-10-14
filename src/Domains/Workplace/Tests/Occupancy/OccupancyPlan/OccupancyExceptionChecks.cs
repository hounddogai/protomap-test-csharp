namespace Acme.Workforce.Workplace.Tests.Occupancy.OccupancyPlan;

using Acme.Workforce.Workplace.Occupancy.OccupancyPlan;

/// <summary>Checks of occupancy plan code, run by the project's runner.</summary>
public static class OccupancyExceptionChecks
{
    public static void OccupancyExceptionStartsInPlanned()
    {
        var item = OccupancyException.NewOccupancyException("code-207", "label-852");
        Check.Equal(item.Status, OccupancyExceptionStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OccupancyExceptionMovesToInProgress()
    {
        var item = OccupancyException.NewOccupancyException("code-207", "label-852");
        try
        {
            item.MoveTo(OccupancyExceptionStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OccupancyExceptionStatus.InProgress, "status");
    }

    public static void OccupancyExceptionRejectsMovesFromCancelled()
    {
        var item = OccupancyException.NewOccupancyException("code-207", "label-852");
        item.Status = OccupancyExceptionStatus.Cancelled;
        Check.Fails(() => item.MoveTo(OccupancyExceptionStatus.Planned), "move");
    }

    public static void RunAll()
    {
        OccupancyExceptionStartsInPlanned();
        OccupancyExceptionMovesToInProgress();
        OccupancyExceptionRejectsMovesFromCancelled();
    }
}
