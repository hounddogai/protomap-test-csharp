namespace Acme.Workforce.Compliance.Tests.Audit.Tier;

using Acme.Workforce.Compliance.Audit.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class AuditTierEngineChecks
{
    public static void AuditTierEngineKeepsResultsWithinBounds()
    {
        var engine = AuditTierEngine.DefaultAuditTierEngine();
        var item = AuditTier.NewAuditTier("code-494", "title-434");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AuditTierEngineKeepsResultsWithinBounds();
    }
}
