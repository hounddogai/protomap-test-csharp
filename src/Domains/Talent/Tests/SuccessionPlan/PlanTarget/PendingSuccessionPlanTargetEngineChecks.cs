namespace Acme.Workforce.Talent.Tests.SuccessionPlan.PlanTarget;

using Acme.Workforce.Talent.SuccessionPlan.PlanTarget;

/// <summary>Checks of plan target code, run by the project's runner.</summary>
public static class PendingSuccessionPlanTargetEngineChecks
{
    public static void PendingSuccessionPlanTargetEngineKeepsResultsWithinBounds()
    {
        var engine = PendingSuccessionPlanTargetEngine.DefaultPendingSuccessionPlanTargetEngine();
        var item = PendingSuccessionPlanTarget.NewPendingSuccessionPlanTarget("code-335", "label-582");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingSuccessionPlanTargetEngineKeepsResultsWithinBounds();
    }
}
