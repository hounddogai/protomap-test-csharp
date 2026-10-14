namespace Acme.Workforce.Absence.Tests.ReturnPlan.Agreement;

using Acme.Workforce.Absence.ReturnPlan.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class ScheduledReturnPlanTargetEngineChecks
{
    public static void ScheduledReturnPlanTargetEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledReturnPlanTargetEngine.DefaultScheduledReturnPlanTargetEngine();
        var item = ScheduledReturnPlanTarget.NewScheduledReturnPlanTarget("code-421", "label-18");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledReturnPlanTargetEngineKeepsResultsWithinBounds();
    }
}
