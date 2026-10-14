namespace Acme.Workforce.Engagement.Tests.ActionPlan.Rollup;

using Acme.Workforce.Engagement.ActionPlan.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class ActionPlanExceptionEngineChecks
{
    public static void ActionPlanExceptionEngineKeepsResultsWithinBounds()
    {
        var engine = ActionPlanExceptionEngine.DefaultActionPlanExceptionEngine();
        var item = ActionPlanException.NewActionPlanException("code-905", "title-867");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ActionPlanExceptionEngineKeepsResultsWithinBounds();
    }
}
