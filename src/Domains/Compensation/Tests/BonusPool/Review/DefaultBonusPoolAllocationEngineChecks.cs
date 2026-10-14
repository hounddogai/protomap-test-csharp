namespace Acme.Workforce.Compensation.Tests.BonusPool.Review;

using Acme.Workforce.Compensation.BonusPool.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class DefaultBonusPoolAllocationEngineChecks
{
    public static void DefaultBonusPoolAllocationEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultBonusPoolAllocationEngine.DefaultDefaultBonusPoolAllocationEngine();
        var item = DefaultBonusPoolAllocation.NewDefaultBonusPoolAllocation("code-62", "name-131");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultBonusPoolAllocationEngineKeepsResultsWithinBounds();
    }
}
