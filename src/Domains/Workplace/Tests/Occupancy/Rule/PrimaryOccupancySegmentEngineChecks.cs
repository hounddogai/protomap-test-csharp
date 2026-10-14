namespace Acme.Workforce.Workplace.Tests.Occupancy.Rule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Occupancy.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class PrimaryOccupancySegmentEngineChecks
{
    public static void PrimaryOccupancySegmentEngineKeepsResultsWithinBounds()
    {
        var engine = PrimaryOccupancySegmentEngine.DefaultPrimaryOccupancySegmentEngine(Flags.NewRegistry());
        var item = PrimaryOccupancySegment.NewPrimaryOccupancySegment("code-649", "label-792");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PrimaryOccupancySegmentEngineKeepsResultsWithinBounds();
    }
}
