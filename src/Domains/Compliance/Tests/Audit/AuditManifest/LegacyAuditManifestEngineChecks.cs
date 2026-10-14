namespace Acme.Workforce.Compliance.Tests.Audit.AuditManifest;

using Acme.Workforce.Compliance.Audit.AuditManifest;

/// <summary>Checks of audit manifest code, run by the project's runner.</summary>
public static class LegacyAuditManifestEngineChecks
{
    public static void LegacyAuditManifestEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyAuditManifestEngine.DefaultLegacyAuditManifestEngine();
        var item = LegacyAuditManifest.NewLegacyAuditManifest("code-674", "name-664");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyAuditManifestEngineKeepsResultsWithinBounds();
    }
}
