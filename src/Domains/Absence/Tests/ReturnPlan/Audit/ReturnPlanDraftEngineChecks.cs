namespace Acme.Workforce.Absence.Tests.ReturnPlan.Audit;

using Acme.Workforce.Absence.ReturnPlan.Audit;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class ReturnPlanDraftEngineChecks
{
    public static void ReturnPlanDraftEngineKeepsResultsWithinBounds()
    {
        var engine = ReturnPlanDraftEngine.DefaultReturnPlanDraftEngine(Flags.NewRegistry());
        var item = ReturnPlanDraft.NewReturnPlanDraft("code-864", "label-671");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ReturnPlanDraftEngineKeepsResultsWithinBounds();
    }
}
