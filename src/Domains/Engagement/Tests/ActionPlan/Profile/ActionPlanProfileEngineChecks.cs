namespace Acme.Workforce.Engagement.Tests.ActionPlan.Profile;

using Acme.Workforce.Engagement.ActionPlan.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class ActionPlanProfileEngineChecks
{
    public static void ActionPlanProfileEngineKeepsResultsWithinBounds()
    {
        var engine = ActionPlanProfileEngine.DefaultActionPlanProfileEngine();
        var item = ActionPlanProfile.NewActionPlanProfile("code-844", "title-493");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ActionPlanProfileEngineKeepsResultsWithinBounds();
    }
}
