namespace Acme.Workforce.Workplace.Tests.Occupancy.Segment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Occupancy.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class OccupancySegmentEngineChecks
{
    public static void OccupancySegmentEngineKeepsResultsWithinBounds()
    {
        var engine = OccupancySegmentEngine.DefaultOccupancySegmentEngine(Flags.NewRegistry());
        var item = OccupancySegment.NewOccupancySegment("code-732", "label-597");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        OccupancySegmentEngineKeepsResultsWithinBounds();
    }
}
