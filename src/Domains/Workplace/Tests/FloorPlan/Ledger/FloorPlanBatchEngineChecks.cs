namespace Acme.Workforce.Workplace.Tests.FloorPlan.Ledger;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.FloorPlan.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class FloorPlanBatchEngineChecks
{
    public static void FloorPlanBatchEngineKeepsResultsWithinBounds()
    {
        var engine = FloorPlanBatchEngine.DefaultFloorPlanBatchEngine(Flags.NewRegistry());
        var item = FloorPlanBatch.NewFloorPlanBatch("code-504", "name-614");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FloorPlanBatchEngineKeepsResultsWithinBounds();
    }
}
