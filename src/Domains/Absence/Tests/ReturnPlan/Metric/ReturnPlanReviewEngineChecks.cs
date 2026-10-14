namespace Acme.Workforce.Absence.Tests.ReturnPlan.Metric;

using Acme.Workforce.Absence.ReturnPlan.Metric;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class ReturnPlanReviewEngineChecks
{
    public static void ReturnPlanReviewEngineKeepsResultsWithinBounds()
    {
        var engine = ReturnPlanReviewEngine.DefaultReturnPlanReviewEngine(Flags.NewRegistry());
        var item = ReturnPlanReview.NewReturnPlanReview("code-929", "name-130");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ReturnPlanReviewEngineKeepsResultsWithinBounds();
    }
}
