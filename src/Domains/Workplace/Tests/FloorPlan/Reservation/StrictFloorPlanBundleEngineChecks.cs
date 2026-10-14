namespace Acme.Workforce.Workplace.Tests.FloorPlan.Reservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.FloorPlan.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class StrictFloorPlanBundleEngineChecks
{
    public static void StrictFloorPlanBundleEngineKeepsResultsWithinBounds()
    {
        var engine = StrictFloorPlanBundleEngine.DefaultStrictFloorPlanBundleEngine(Flags.NewRegistry());
        var item = StrictFloorPlanBundle.NewStrictFloorPlanBundle("code-528", "name-424");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictFloorPlanBundleEngineKeepsResultsWithinBounds();
    }
}
