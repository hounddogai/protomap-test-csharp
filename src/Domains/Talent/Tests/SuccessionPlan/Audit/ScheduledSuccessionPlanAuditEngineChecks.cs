namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Audit;

using Acme.Workforce.Talent.SuccessionPlan.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class ScheduledSuccessionPlanAuditEngineChecks
{
    public static void ScheduledSuccessionPlanAuditEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledSuccessionPlanAuditEngine.DefaultScheduledSuccessionPlanAuditEngine();
        var item = ScheduledSuccessionPlanAudit.NewScheduledSuccessionPlanAudit("code-589", "title-971");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledSuccessionPlanAuditEngineKeepsResultsWithinBounds();
    }
}
