namespace Acme.Workforce.Workplace.Tests.Occupancy.Segment;

using Acme.Workforce.Workplace.Occupancy.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class OccupancySegmentChecks
{
    public static void OccupancySegmentStartsInCreated()
    {
        var item = OccupancySegment.NewOccupancySegment("code-732", "label-597");
        Check.Equal(item.Status, OccupancySegmentStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OccupancySegmentMovesToValidated()
    {
        var item = OccupancySegment.NewOccupancySegment("code-732", "label-597");
        try
        {
            item.MoveTo(OccupancySegmentStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OccupancySegmentStatus.Validated, "status");
    }

    public static void OccupancySegmentRejectsMovesFromRetired()
    {
        var item = OccupancySegment.NewOccupancySegment("code-732", "label-597");
        item.Status = OccupancySegmentStatus.Retired;
        Check.Fails(() => item.MoveTo(OccupancySegmentStatus.Created), "move");
    }

    public static void RunAll()
    {
        OccupancySegmentStartsInCreated();
        OccupancySegmentMovesToValidated();
        OccupancySegmentRejectsMovesFromRetired();
    }
}
