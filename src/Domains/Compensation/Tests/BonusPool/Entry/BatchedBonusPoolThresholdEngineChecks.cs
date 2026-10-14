namespace Acme.Workforce.Compensation.Tests.BonusPool.Entry;

using Acme.Workforce.Compensation.BonusPool.Entry;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class BatchedBonusPoolThresholdEngineChecks
{
    public static void BatchedBonusPoolThresholdEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedBonusPoolThresholdEngine.DefaultBatchedBonusPoolThresholdEngine(Flags.NewRegistry());
        var item = BatchedBonusPoolThreshold.NewBatchedBonusPoolThreshold("code-983", "title-737");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedBonusPoolThresholdEngineKeepsResultsWithinBounds();
    }
}
