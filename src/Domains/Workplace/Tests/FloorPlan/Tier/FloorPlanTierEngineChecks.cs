namespace Acme.Workforce.Workplace.Tests.FloorPlan.Tier;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.FloorPlan.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class FloorPlanTierEngineChecks
{
    public static void FloorPlanTierEngineKeepsResultsWithinBounds()
    {
        var engine = FloorPlanTierEngine.DefaultFloorPlanTierEngine(Flags.NewRegistry());
        var item = FloorPlanTier.NewFloorPlanTier("code-200", "name-625");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FloorPlanTierEngineKeepsResultsWithinBounds();
    }
}
