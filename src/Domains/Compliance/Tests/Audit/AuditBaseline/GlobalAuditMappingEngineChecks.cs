namespace Acme.Workforce.Compliance.Tests.Audit.AuditBaseline;

using Acme.Workforce.Compliance.Audit.AuditBaseline;

/// <summary>Checks of audit baseline code, run by the project's runner.</summary>
public static class GlobalAuditMappingEngineChecks
{
    public static void GlobalAuditMappingEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalAuditMappingEngine.DefaultGlobalAuditMappingEngine();
        var item = GlobalAuditMapping.NewGlobalAuditMapping("code-719", "name-356");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalAuditMappingEngineKeepsResultsWithinBounds();
    }
}
