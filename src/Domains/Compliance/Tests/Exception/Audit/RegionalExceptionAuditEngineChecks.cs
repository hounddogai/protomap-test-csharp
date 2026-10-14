namespace Acme.Workforce.Compliance.Tests.Exception.Audit;

using Acme.Workforce.Compliance.Exception.Audit;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class RegionalExceptionAuditEngineChecks
{
    public static void RegionalExceptionAuditEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalExceptionAuditEngine.DefaultRegionalExceptionAuditEngine(Flags.NewRegistry());
        var item = RegionalExceptionAudit.NewRegionalExceptionAudit("code-972", "name-764");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalExceptionAuditEngineKeepsResultsWithinBounds();
    }
}
