namespace Acme.Workforce.Workplace.Tests.Occupancy.Rule;

using Acme.Workforce.Workplace.Occupancy.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class PrimaryOccupancySegmentChecks
{
    public static void PrimaryOccupancySegmentStartsInProposed()
    {
        var item = PrimaryOccupancySegment.NewPrimaryOccupancySegment("code-649", "label-792");
        Check.Equal(item.Status, PrimaryOccupancySegmentStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryOccupancySegmentMovesToApproved()
    {
        var item = PrimaryOccupancySegment.NewPrimaryOccupancySegment("code-649", "label-792");
        try
        {
            item.MoveTo(PrimaryOccupancySegmentStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryOccupancySegmentStatus.Approved, "status");
    }

    public static void PrimaryOccupancySegmentRejectsMovesFromWithdrawn()
    {
        var item = PrimaryOccupancySegment.NewPrimaryOccupancySegment("code-649", "label-792");
        item.Status = PrimaryOccupancySegmentStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(PrimaryOccupancySegmentStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        PrimaryOccupancySegmentStartsInProposed();
        PrimaryOccupancySegmentMovesToApproved();
        PrimaryOccupancySegmentRejectsMovesFromWithdrawn();
    }
}
