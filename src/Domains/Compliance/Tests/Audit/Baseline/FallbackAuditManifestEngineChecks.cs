namespace Acme.Workforce.Compliance.Tests.Audit.Baseline;

using Acme.Workforce.Compliance.Audit.Baseline;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class FallbackAuditManifestEngineChecks
{
    public static void FallbackAuditManifestEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackAuditManifestEngine.DefaultFallbackAuditManifestEngine(Flags.NewRegistry());
        var item = FallbackAuditManifest.NewFallbackAuditManifest("code-104", "label-729");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackAuditManifestEngineKeepsResultsWithinBounds();
    }
}
