namespace Acme.Workforce.Workplace.Tests.FloorPlan.Milestone;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.FloorPlan.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class PendingFloorPlanTargetEngineChecks
{
    public static void PendingFloorPlanTargetEngineKeepsResultsWithinBounds()
    {
        var engine = PendingFloorPlanTargetEngine.DefaultPendingFloorPlanTargetEngine(Flags.NewRegistry());
        var item = PendingFloorPlanTarget.NewPendingFloorPlanTarget("code-427", "label-236");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingFloorPlanTargetEngineKeepsResultsWithinBounds();
    }
}
