namespace Acme.Workforce.Compliance.Tests.Audit.AuditRecord;

using Acme.Workforce.Compliance.Audit.AuditRecord;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of audit record code, run by the project's runner.</summary>
public static class BatchedAuditCycleEngineChecks
{
    public static void BatchedAuditCycleEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedAuditCycleEngine.DefaultBatchedAuditCycleEngine(Flags.NewRegistry());
        var item = BatchedAuditCycle.NewBatchedAuditCycle("code-221", "name-19");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedAuditCycleEngineKeepsResultsWithinBounds();
    }
}
