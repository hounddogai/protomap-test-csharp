namespace Acme.Workforce.Workplace.Tests.Occupancy.Score;

using Acme.Workforce.Workplace.Occupancy.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class OccupancyScheduleChecks
{
    public static void OccupancyScheduleStartsInPlanned()
    {
        var item = OccupancySchedule.NewOccupancySchedule("code-646", "title-947");
        Check.Equal(item.Status, OccupancyScheduleStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OccupancyScheduleMovesToScheduled()
    {
        var item = OccupancySchedule.NewOccupancySchedule("code-646", "title-947");
        try
        {
            item.MoveTo(OccupancyScheduleStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OccupancyScheduleStatus.Scheduled, "status");
    }

    public static void OccupancyScheduleRejectsMovesFromCancelled()
    {
        var item = OccupancySchedule.NewOccupancySchedule("code-646", "title-947");
        item.Status = OccupancyScheduleStatus.Cancelled;
        Check.Fails(() => item.MoveTo(OccupancyScheduleStatus.Planned), "move");
    }

    public static void RunAll()
    {
        OccupancyScheduleStartsInPlanned();
        OccupancyScheduleMovesToScheduled();
        OccupancyScheduleRejectsMovesFromCancelled();
    }
}
