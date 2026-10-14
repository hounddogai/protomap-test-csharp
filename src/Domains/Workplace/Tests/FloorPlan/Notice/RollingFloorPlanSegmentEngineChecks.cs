namespace Acme.Workforce.Workplace.Tests.FloorPlan.Notice;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.FloorPlan.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class RollingFloorPlanSegmentEngineChecks
{
    public static void RollingFloorPlanSegmentEngineKeepsResultsWithinBounds()
    {
        var engine = RollingFloorPlanSegmentEngine.DefaultRollingFloorPlanSegmentEngine(Flags.NewRegistry());
        var item = RollingFloorPlanSegment.NewRollingFloorPlanSegment("code-61", "name-220");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RollingFloorPlanSegmentEngineKeepsResultsWithinBounds();
    }
}
