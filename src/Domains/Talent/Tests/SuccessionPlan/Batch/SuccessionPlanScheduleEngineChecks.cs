namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Batch;

using Acme.Workforce.Talent.SuccessionPlan.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class SuccessionPlanScheduleEngineChecks
{
    public static void SuccessionPlanScheduleEngineKeepsResultsWithinBounds()
    {
        var engine = SuccessionPlanScheduleEngine.DefaultSuccessionPlanScheduleEngine();
        var item = SuccessionPlanSchedule.NewSuccessionPlanSchedule("code-387", "label-967");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SuccessionPlanScheduleEngineKeepsResultsWithinBounds();
    }
}
