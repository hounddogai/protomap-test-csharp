namespace Acme.Workforce.Compliance.Tests.Audit.Mapping;

using Acme.Workforce.Compliance.Audit.Mapping;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class AuditMappingEngineChecks
{
    public static void AuditMappingEngineKeepsResultsWithinBounds()
    {
        var engine = AuditMappingEngine.DefaultAuditMappingEngine(Flags.NewRegistry());
        var item = AuditMapping.NewAuditMapping("code-918", "name-9");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AuditMappingEngineKeepsResultsWithinBounds();
    }
}
