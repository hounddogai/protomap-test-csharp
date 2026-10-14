namespace Acme.Workforce.Orgdesign.Tests.Headcount.Assignment;

using Acme.Workforce.Orgdesign.Headcount.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class HeadcountAuditEngineChecks
{
    public static void HeadcountAuditEngineKeepsResultsWithinBounds()
    {
        var engine = HeadcountAuditEngine.DefaultHeadcountAuditEngine();
        var item = HeadcountAudit.NewHeadcountAudit("code-764", "label-232");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        HeadcountAuditEngineKeepsResultsWithinBounds();
    }
}
