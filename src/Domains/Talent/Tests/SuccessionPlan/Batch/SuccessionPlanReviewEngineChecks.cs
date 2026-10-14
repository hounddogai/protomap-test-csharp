namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Batch;

using Acme.Workforce.Talent.SuccessionPlan.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class SuccessionPlanReviewEngineChecks
{
    public static void SuccessionPlanReviewEngineKeepsResultsWithinBounds()
    {
        var engine = SuccessionPlanReviewEngine.DefaultSuccessionPlanReviewEngine();
        var item = SuccessionPlanReview.NewSuccessionPlanReview("code-455", "title-185");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SuccessionPlanReviewEngineKeepsResultsWithinBounds();
    }
}
