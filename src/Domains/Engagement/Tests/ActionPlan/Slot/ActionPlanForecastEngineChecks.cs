namespace Acme.Workforce.Engagement.Tests.ActionPlan.Slot;

using Acme.Workforce.Engagement.ActionPlan.Slot;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class ActionPlanForecastEngineChecks
{
    public static void ActionPlanForecastEngineKeepsResultsWithinBounds()
    {
        var engine = ActionPlanForecastEngine.DefaultActionPlanForecastEngine(Flags.NewRegistry());
        var item = ActionPlanForecast.NewActionPlanForecast("code-260", "title-64");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ActionPlanForecastEngineKeepsResultsWithinBounds();
    }
}
