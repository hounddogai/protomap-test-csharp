namespace Acme.Workforce.Compensation.Tests.Adjustment.Allocation;

using Acme.Workforce.Compensation.Adjustment.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class AdjustmentBaselineEngineChecks
{
    public static void AdjustmentBaselineEngineKeepsResultsWithinBounds()
    {
        var engine = AdjustmentBaselineEngine.DefaultAdjustmentBaselineEngine();
        var item = AdjustmentBaseline.NewAdjustmentBaseline("code-910", "title-989");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdjustmentBaselineEngineKeepsResultsWithinBounds();
    }
}
