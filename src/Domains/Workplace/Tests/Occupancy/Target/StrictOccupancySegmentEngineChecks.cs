namespace Acme.Workforce.Workplace.Tests.Occupancy.Target;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Occupancy.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class StrictOccupancySegmentEngineChecks
{
    public static void StrictOccupancySegmentEngineKeepsResultsWithinBounds()
    {
        var engine = StrictOccupancySegmentEngine.DefaultStrictOccupancySegmentEngine(Flags.NewRegistry());
        var item = StrictOccupancySegment.NewStrictOccupancySegment("code-95", "label-956");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictOccupancySegmentEngineKeepsResultsWithinBounds();
    }
}
