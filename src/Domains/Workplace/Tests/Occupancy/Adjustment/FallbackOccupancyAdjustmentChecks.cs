namespace Acme.Workforce.Workplace.Tests.Occupancy.Adjustment;

using Acme.Workforce.Workplace.Occupancy.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class FallbackOccupancyAdjustmentChecks
{
    public static void FallbackOccupancyAdjustmentStartsInNew()
    {
        var item = FallbackOccupancyAdjustment.NewFallbackOccupancyAdjustment("code-862", "label-726");
        Check.Equal(item.Status, FallbackOccupancyAdjustmentStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackOccupancyAdjustmentMovesToQueued()
    {
        var item = FallbackOccupancyAdjustment.NewFallbackOccupancyAdjustment("code-862", "label-726");
        try
        {
            item.MoveTo(FallbackOccupancyAdjustmentStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackOccupancyAdjustmentStatus.Queued, "status");
    }

    public static void FallbackOccupancyAdjustmentRejectsMovesFromFailed()
    {
        var item = FallbackOccupancyAdjustment.NewFallbackOccupancyAdjustment("code-862", "label-726");
        item.Status = FallbackOccupancyAdjustmentStatus.Failed;
        Check.Fails(() => item.MoveTo(FallbackOccupancyAdjustmentStatus.New), "move");
    }

    public static void RunAll()
    {
        FallbackOccupancyAdjustmentStartsInNew();
        FallbackOccupancyAdjustmentMovesToQueued();
        FallbackOccupancyAdjustmentRejectsMovesFromFailed();
    }
}
