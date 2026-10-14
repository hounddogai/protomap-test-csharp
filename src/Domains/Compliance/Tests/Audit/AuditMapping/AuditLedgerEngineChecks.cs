namespace Acme.Workforce.Compliance.Tests.Audit.AuditMapping;

using Acme.Workforce.Compliance.Audit.AuditMapping;

/// <summary>Checks of audit mapping code, run by the project's runner.</summary>
public static class AuditLedgerEngineChecks
{
    public static void AuditLedgerEngineKeepsResultsWithinBounds()
    {
        var engine = AuditLedgerEngine.DefaultAuditLedgerEngine();
        var item = AuditLedger.NewAuditLedger("code-298", "name-204");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AuditLedgerEngineKeepsResultsWithinBounds();
    }
}
