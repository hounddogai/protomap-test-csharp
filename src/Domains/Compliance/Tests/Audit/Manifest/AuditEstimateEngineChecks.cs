namespace Acme.Workforce.Compliance.Tests.Audit.Manifest;

using Acme.Workforce.Compliance.Audit.Manifest;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class AuditEstimateEngineChecks
{
    public static void AuditEstimateEngineKeepsResultsWithinBounds()
    {
        var engine = AuditEstimateEngine.DefaultAuditEstimateEngine(Flags.NewRegistry());
        var item = AuditEstimate.NewAuditEstimate("code-527", "name-10");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AuditEstimateEngineKeepsResultsWithinBounds();
    }
}
