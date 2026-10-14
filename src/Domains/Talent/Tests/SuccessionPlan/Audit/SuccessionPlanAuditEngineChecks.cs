namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Audit;

using Acme.Workforce.Talent.SuccessionPlan.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class SuccessionPlanAuditEngineChecks
{
    public static void SuccessionPlanAuditEngineKeepsResultsWithinBounds()
    {
        var engine = SuccessionPlanAuditEngine.DefaultSuccessionPlanAuditEngine();
        var item = SuccessionPlanAudit.NewSuccessionPlanAudit("code-166", "name-870");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SuccessionPlanAuditEngineKeepsResultsWithinBounds();
    }
}
