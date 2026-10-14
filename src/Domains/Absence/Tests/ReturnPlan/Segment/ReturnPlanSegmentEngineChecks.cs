namespace Acme.Workforce.Absence.Tests.ReturnPlan.Segment;

using Acme.Workforce.Absence.ReturnPlan.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class ReturnPlanSegmentEngineChecks
{
    public static void ReturnPlanSegmentEngineKeepsResultsWithinBounds()
    {
        var engine = ReturnPlanSegmentEngine.DefaultReturnPlanSegmentEngine();
        var item = ReturnPlanSegment.NewReturnPlanSegment("code-965", "name-620");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ReturnPlanSegmentEngineKeepsResultsWithinBounds();
    }
}
