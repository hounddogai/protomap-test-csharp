namespace Acme.Workforce.Compliance.Tests.Audit.Draft;

using Acme.Workforce.Compliance.Audit.Draft;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class AuditRequestEngineChecks
{
    public static void AuditRequestEngineKeepsResultsWithinBounds()
    {
        var engine = AuditRequestEngine.DefaultAuditRequestEngine(Flags.NewRegistry());
        var item = AuditRequest.NewAuditRequest("code-870", "label-798");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AuditRequestEngineKeepsResultsWithinBounds();
    }
}
