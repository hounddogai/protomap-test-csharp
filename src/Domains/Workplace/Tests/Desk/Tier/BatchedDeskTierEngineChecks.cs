namespace Acme.Workforce.Workplace.Tests.Desk.Tier;

using Acme.Workforce.Workplace.Desk.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class BatchedDeskTierEngineChecks
{
    public static void BatchedDeskTierEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedDeskTierEngine.DefaultBatchedDeskTierEngine();
        var item = BatchedDeskTier.NewBatchedDeskTier("code-381", "label-115");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedDeskTierEngineKeepsResultsWithinBounds();
    }
}
