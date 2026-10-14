namespace Acme.Workforce.Workplace.Tests.Occupancy.Score;

using Acme.Workforce.Workplace.Occupancy.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class OccupancyAdjustmentChecks
{
    public static void OccupancyAdjustmentStartsInDraft()
    {
        var item = OccupancyAdjustment.NewOccupancyAdjustment("code-363", "title-454");
        Check.Equal(item.Status, OccupancyAdjustmentStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OccupancyAdjustmentMovesToClosed()
    {
        var item = OccupancyAdjustment.NewOccupancyAdjustment("code-363", "title-454");
        try
        {
            item.MoveTo(OccupancyAdjustmentStatus.Closed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OccupancyAdjustmentStatus.Closed, "status");
    }

    public static void OccupancyAdjustmentRejectsMovesFromClosed()
    {
        var item = OccupancyAdjustment.NewOccupancyAdjustment("code-363", "title-454");
        item.Status = OccupancyAdjustmentStatus.Closed;
        Check.Fails(() => item.MoveTo(OccupancyAdjustmentStatus.Draft), "move");
    }

    public static void RunAll()
    {
        OccupancyAdjustmentStartsInDraft();
        OccupancyAdjustmentMovesToClosed();
        OccupancyAdjustmentRejectsMovesFromClosed();
    }
}
