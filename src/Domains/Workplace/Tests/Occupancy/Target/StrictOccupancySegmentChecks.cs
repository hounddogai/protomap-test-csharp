namespace Acme.Workforce.Workplace.Tests.Occupancy.Target;

using Acme.Workforce.Workplace.Occupancy.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class StrictOccupancySegmentChecks
{
    public static void StrictOccupancySegmentStartsInOpen()
    {
        var item = StrictOccupancySegment.NewStrictOccupancySegment("code-95", "label-956");
        Check.Equal(item.Status, StrictOccupancySegmentStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictOccupancySegmentMovesToResolved()
    {
        var item = StrictOccupancySegment.NewStrictOccupancySegment("code-95", "label-956");
        try
        {
            item.MoveTo(StrictOccupancySegmentStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictOccupancySegmentStatus.Resolved, "status");
    }

    public static void StrictOccupancySegmentRejectsMovesFromArchived()
    {
        var item = StrictOccupancySegment.NewStrictOccupancySegment("code-95", "label-956");
        item.Status = StrictOccupancySegmentStatus.Archived;
        Check.Fails(() => item.MoveTo(StrictOccupancySegmentStatus.Open), "move");
    }

    public static void RunAll()
    {
        StrictOccupancySegmentStartsInOpen();
        StrictOccupancySegmentMovesToResolved();
        StrictOccupancySegmentRejectsMovesFromArchived();
    }
}
