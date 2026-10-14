namespace Acme.Workforce.Workplace.Tests.Occupancy.Adjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Occupancy.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class FallbackOccupancyAdjustmentEngineChecks
{
    public static void FallbackOccupancyAdjustmentEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackOccupancyAdjustmentEngine.DefaultFallbackOccupancyAdjustmentEngine(Flags.NewRegistry());
        var item = FallbackOccupancyAdjustment.NewFallbackOccupancyAdjustment("code-862", "label-726");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackOccupancyAdjustmentEngineKeepsResultsWithinBounds();
    }
}
