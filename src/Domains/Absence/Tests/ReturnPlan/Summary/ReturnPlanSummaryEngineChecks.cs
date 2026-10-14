namespace Acme.Workforce.Absence.Tests.ReturnPlan.Summary;

using Acme.Workforce.Absence.ReturnPlan.Summary;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class ReturnPlanSummaryEngineChecks
{
    public static void ReturnPlanSummaryEngineKeepsResultsWithinBounds()
    {
        var engine = ReturnPlanSummaryEngine.DefaultReturnPlanSummaryEngine(Flags.NewRegistry());
        var item = ReturnPlanSummary.NewReturnPlanSummary("code-389", "name-184");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ReturnPlanSummaryEngineKeepsResultsWithinBounds();
    }
}
