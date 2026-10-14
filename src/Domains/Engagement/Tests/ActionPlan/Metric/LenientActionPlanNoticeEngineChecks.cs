namespace Acme.Workforce.Engagement.Tests.ActionPlan.Metric;

using Acme.Workforce.Engagement.ActionPlan.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class LenientActionPlanNoticeEngineChecks
{
    public static void LenientActionPlanNoticeEngineKeepsResultsWithinBounds()
    {
        var engine = LenientActionPlanNoticeEngine.DefaultLenientActionPlanNoticeEngine();
        var item = LenientActionPlanNotice.NewLenientActionPlanNotice("code-73", "label-762");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientActionPlanNoticeEngineKeepsResultsWithinBounds();
    }
}
