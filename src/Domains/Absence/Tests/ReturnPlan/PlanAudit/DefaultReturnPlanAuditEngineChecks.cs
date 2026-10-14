namespace Acme.Workforce.Absence.Tests.ReturnPlan.PlanAudit;

using Acme.Workforce.Absence.ReturnPlan.PlanAudit;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of plan audit code, run by the project's runner.</summary>
public static class DefaultReturnPlanAuditEngineChecks
{
    public static void DefaultReturnPlanAuditEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultReturnPlanAuditEngine.DefaultDefaultReturnPlanAuditEngine(Flags.NewRegistry());
        var item = DefaultReturnPlanAudit.NewDefaultReturnPlanAudit("code-677", "title-921");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultReturnPlanAuditEngineKeepsResultsWithinBounds();
    }
}
