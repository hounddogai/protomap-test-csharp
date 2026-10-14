namespace Acme.Workforce.Absence.Tests.ReturnPlan.Segment;

using Acme.Workforce.Absence.ReturnPlan.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class StrictReturnPlanBundleEngineChecks
{
    public static void StrictReturnPlanBundleEngineKeepsResultsWithinBounds()
    {
        var engine = StrictReturnPlanBundleEngine.DefaultStrictReturnPlanBundleEngine();
        var item = StrictReturnPlanBundle.NewStrictReturnPlanBundle("code-401", "name-834");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictReturnPlanBundleEngineKeepsResultsWithinBounds();
    }
}
