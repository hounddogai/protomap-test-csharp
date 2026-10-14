namespace Acme.Workforce.Absence.Tests.ReturnPlan.Tier;

using Acme.Workforce.Absence.ReturnPlan.Tier;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class BatchedReturnPlanLedgerEngineChecks
{
    public static void BatchedReturnPlanLedgerEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedReturnPlanLedgerEngine.DefaultBatchedReturnPlanLedgerEngine(Flags.NewRegistry());
        var item = BatchedReturnPlanLedger.NewBatchedReturnPlanLedger("code-338", "name-811");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedReturnPlanLedgerEngineKeepsResultsWithinBounds();
    }
}
