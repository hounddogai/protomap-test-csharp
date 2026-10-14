namespace Acme.Workforce.Absence.Tests.Balance.Batch;

using Acme.Workforce.Absence.Balance.Batch;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class BalanceBatchEngineChecks
{
    public static void BalanceBatchEngineKeepsResultsWithinBounds()
    {
        var engine = BalanceBatchEngine.DefaultBalanceBatchEngine(Flags.NewRegistry());
        var item = BalanceBatch.NewBalanceBatch("code-748", "name-134");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BalanceBatchEngineKeepsResultsWithinBounds();
    }
}
