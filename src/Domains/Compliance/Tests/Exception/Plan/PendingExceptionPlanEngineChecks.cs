namespace Acme.Workforce.Compliance.Tests.Exception.Plan;

using Acme.Workforce.Compliance.Exception.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class PendingExceptionPlanEngineChecks
{
    public static void PendingExceptionPlanEngineKeepsResultsWithinBounds()
    {
        var engine = PendingExceptionPlanEngine.DefaultPendingExceptionPlanEngine();
        var item = PendingExceptionPlan.NewPendingExceptionPlan("code-812", "title-271");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingExceptionPlanEngineKeepsResultsWithinBounds();
    }
}
