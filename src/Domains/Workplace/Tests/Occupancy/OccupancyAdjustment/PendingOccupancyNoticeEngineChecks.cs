namespace Acme.Workforce.Workplace.Tests.Occupancy.OccupancyAdjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Occupancy.OccupancyAdjustment;

/// <summary>Checks of occupancy adjustment code, run by the project's runner.</summary>
public static class PendingOccupancyNoticeEngineChecks
{
    public static void PendingOccupancyNoticeEngineKeepsResultsWithinBounds()
    {
        var engine = PendingOccupancyNoticeEngine.DefaultPendingOccupancyNoticeEngine(Flags.NewRegistry());
        var item = PendingOccupancyNotice.NewPendingOccupancyNotice("code-659", "name-670");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingOccupancyNoticeEngineKeepsResultsWithinBounds();
    }
}
