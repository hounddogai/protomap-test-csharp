namespace Acme.Workforce.Workplace.Tests.FloorPlan.Rollup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.FloorPlan.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class RollingFloorPlanRollupEngineChecks
{
    public static void RollingFloorPlanRollupEngineKeepsResultsWithinBounds()
    {
        var engine = RollingFloorPlanRollupEngine.DefaultRollingFloorPlanRollupEngine(Flags.NewRegistry());
        var item = RollingFloorPlanRollup.NewRollingFloorPlanRollup("code-119", "name-745");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RollingFloorPlanRollupEngineKeepsResultsWithinBounds();
    }
}
