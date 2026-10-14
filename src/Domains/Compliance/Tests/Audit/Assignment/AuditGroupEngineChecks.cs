namespace Acme.Workforce.Compliance.Tests.Audit.Assignment;

using Acme.Workforce.Compliance.Audit.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class AuditGroupEngineChecks
{
    public static void AuditGroupEngineKeepsResultsWithinBounds()
    {
        var engine = AuditGroupEngine.DefaultAuditGroupEngine();
        var item = AuditGroup.NewAuditGroup("code-325", "label-974");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AuditGroupEngineKeepsResultsWithinBounds();
    }
}
