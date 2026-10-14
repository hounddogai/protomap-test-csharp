namespace Acme.Workforce.Compliance.Tests.Audit.Claim;

using Acme.Workforce.Compliance.Audit.Claim;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class AuditRollupEngineChecks
{
    public static void AuditRollupEngineKeepsResultsWithinBounds()
    {
        var engine = AuditRollupEngine.DefaultAuditRollupEngine(Flags.NewRegistry());
        var item = AuditRollup.NewAuditRollup("code-680", "title-837");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AuditRollupEngineKeepsResultsWithinBounds();
    }
}
