namespace Acme.Workforce.Workplace.Tests.Occupancy.Target;

using Acme.Workforce.Workplace.Occupancy.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class StrictOccupancyAdjustmentChecks
{
    public static void StrictOccupancyAdjustmentStartsInOpen()
    {
        var item = StrictOccupancyAdjustment.NewStrictOccupancyAdjustment("code-213", "label-201");
        Check.Equal(item.Status, StrictOccupancyAdjustmentStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictOccupancyAdjustmentMovesToInReview()
    {
        var item = StrictOccupancyAdjustment.NewStrictOccupancyAdjustment("code-213", "label-201");
        try
        {
            item.MoveTo(StrictOccupancyAdjustmentStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictOccupancyAdjustmentStatus.InReview, "status");
    }

    public static void StrictOccupancyAdjustmentRejectsMovesFromArchived()
    {
        var item = StrictOccupancyAdjustment.NewStrictOccupancyAdjustment("code-213", "label-201");
        item.Status = StrictOccupancyAdjustmentStatus.Archived;
        Check.Fails(() => item.MoveTo(StrictOccupancyAdjustmentStatus.Open), "move");
    }

    public static void RunAll()
    {
        StrictOccupancyAdjustmentStartsInOpen();
        StrictOccupancyAdjustmentMovesToInReview();
        StrictOccupancyAdjustmentRejectsMovesFromArchived();
    }
}
