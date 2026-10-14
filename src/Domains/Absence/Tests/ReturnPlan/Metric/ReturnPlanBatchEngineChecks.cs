namespace Acme.Workforce.Absence.Tests.ReturnPlan.Metric;

using Acme.Workforce.Absence.ReturnPlan.Metric;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class ReturnPlanBatchEngineChecks
{
    public static void ReturnPlanBatchEngineKeepsResultsWithinBounds()
    {
        var engine = ReturnPlanBatchEngine.DefaultReturnPlanBatchEngine(Flags.NewRegistry());
        var item = ReturnPlanBatch.NewReturnPlanBatch("code-321", "label-494");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ReturnPlanBatchEngineKeepsResultsWithinBounds();
    }
}
