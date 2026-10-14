namespace Acme.Workforce.Compensation.Tests.Adjustment.Allocation;

using Acme.Workforce.Compensation.Adjustment.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class AdjustmentAllocationEngineChecks
{
    public static void AdjustmentAllocationEngineKeepsResultsWithinBounds()
    {
        var engine = AdjustmentAllocationEngine.DefaultAdjustmentAllocationEngine();
        var item = AdjustmentAllocation.NewAdjustmentAllocation("code-181", "name-764");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdjustmentAllocationEngineKeepsResultsWithinBounds();
    }
}
