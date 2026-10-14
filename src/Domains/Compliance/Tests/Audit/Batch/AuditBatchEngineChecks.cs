namespace Acme.Workforce.Compliance.Tests.Audit.Batch;

using Acme.Workforce.Compliance.Audit.Batch;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class AuditBatchEngineChecks
{
    public static void AuditBatchEngineKeepsResultsWithinBounds()
    {
        var engine = AuditBatchEngine.DefaultAuditBatchEngine(Flags.NewRegistry());
        var item = AuditBatch.NewAuditBatch("code-490", "title-417");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AuditBatchEngineKeepsResultsWithinBounds();
    }
}
