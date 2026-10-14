namespace Acme.Workforce.Compliance.Tests.Audit.Digest;

using Acme.Workforce.Compliance.Audit.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class AuditQuotaEngineChecks
{
    public static void AuditQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = AuditQuotaEngine.DefaultAuditQuotaEngine();
        var item = AuditQuota.NewAuditQuota("code-589", "name-908");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AuditQuotaEngineKeepsResultsWithinBounds();
    }
}
