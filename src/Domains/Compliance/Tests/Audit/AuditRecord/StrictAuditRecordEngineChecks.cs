namespace Acme.Workforce.Compliance.Tests.Audit.AuditRecord;

using Acme.Workforce.Compliance.Audit.AuditRecord;

/// <summary>Checks of audit record code, run by the project's runner.</summary>
public static class StrictAuditRecordEngineChecks
{
    public static void StrictAuditRecordEngineKeepsResultsWithinBounds()
    {
        var engine = StrictAuditRecordEngine.DefaultStrictAuditRecordEngine();
        var item = StrictAuditRecord.NewStrictAuditRecord("code-591", "title-863");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictAuditRecordEngineKeepsResultsWithinBounds();
    }
}
