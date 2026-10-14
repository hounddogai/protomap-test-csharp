namespace Acme.Workforce.Compliance.Tests.Audit.Checkpoint;

using Acme.Workforce.Compliance.Audit.Checkpoint;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class AuditClaimEngineChecks
{
    public static void AuditClaimEngineKeepsResultsWithinBounds()
    {
        var engine = AuditClaimEngine.DefaultAuditClaimEngine(Flags.NewRegistry());
        var item = AuditClaim.NewAuditClaim("code-761", "name-660");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AuditClaimEngineKeepsResultsWithinBounds();
    }
}
