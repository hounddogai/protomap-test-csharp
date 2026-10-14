namespace Acme.Workforce.Compliance.Tests.Audit.AuditRecord;

using Acme.Workforce.Compliance.Audit.AuditRecord;

/// <summary>Checks of audit record code, run by the project's runner.</summary>
public static class CappedAuditCycleEngineChecks
{
    public static void CappedAuditCycleEngineKeepsResultsWithinBounds()
    {
        var engine = CappedAuditCycleEngine.DefaultCappedAuditCycleEngine();
        var item = CappedAuditCycle.NewCappedAuditCycle("code-770", "label-565");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedAuditCycleEngineKeepsResultsWithinBounds();
    }
}
