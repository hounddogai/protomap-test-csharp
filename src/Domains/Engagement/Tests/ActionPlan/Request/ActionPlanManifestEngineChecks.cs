namespace Acme.Workforce.Engagement.Tests.ActionPlan.Request;

using Acme.Workforce.Engagement.ActionPlan.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class ActionPlanManifestEngineChecks
{
    public static void ActionPlanManifestEngineKeepsResultsWithinBounds()
    {
        var engine = ActionPlanManifestEngine.DefaultActionPlanManifestEngine();
        var item = ActionPlanManifest.NewActionPlanManifest("code-535", "title-802");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ActionPlanManifestEngineKeepsResultsWithinBounds();
    }
}
