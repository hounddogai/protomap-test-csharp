namespace Acme.Workforce.Engagement.Tests.ActionPlan.Bundle;

using Acme.Workforce.Engagement.ActionPlan.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class ActionPlanReservationEngineChecks
{
    public static void ActionPlanReservationEngineKeepsResultsWithinBounds()
    {
        var engine = ActionPlanReservationEngine.DefaultActionPlanReservationEngine();
        var item = ActionPlanReservation.NewActionPlanReservation("code-13", "title-987");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ActionPlanReservationEngineKeepsResultsWithinBounds();
    }
}
