namespace Acme.Workforce.Absence.Tests.ReturnPlan.Schedule;

using Acme.Workforce.Absence.ReturnPlan.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class ReturnPlanThresholdEngineChecks
{
    public static void ReturnPlanThresholdEngineKeepsResultsWithinBounds()
    {
        var engine = ReturnPlanThresholdEngine.DefaultReturnPlanThresholdEngine();
        var item = ReturnPlanThreshold.NewReturnPlanThreshold("code-988", "title-497");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ReturnPlanThresholdEngineKeepsResultsWithinBounds();
    }
}
