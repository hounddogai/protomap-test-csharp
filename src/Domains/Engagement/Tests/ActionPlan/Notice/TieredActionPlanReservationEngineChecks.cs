namespace Acme.Workforce.Engagement.Tests.ActionPlan.Notice;

using Acme.Workforce.Engagement.ActionPlan.Notice;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class TieredActionPlanReservationEngineChecks
{
    public static void TieredActionPlanReservationEngineKeepsResultsWithinBounds()
    {
        var engine = TieredActionPlanReservationEngine.DefaultTieredActionPlanReservationEngine(Flags.NewRegistry());
        var item = TieredActionPlanReservation.NewTieredActionPlanReservation("code-161", "name-536");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredActionPlanReservationEngineKeepsResultsWithinBounds();
    }
}
