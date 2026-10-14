namespace Acme.Workforce.Workplace.Tests.FloorPlan.Adjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.FloorPlan.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class FloorPlanReservationEngineChecks
{
    public static void FloorPlanReservationEngineKeepsResultsWithinBounds()
    {
        var engine = FloorPlanReservationEngine.DefaultFloorPlanReservationEngine(Flags.NewRegistry());
        var item = FloorPlanReservation.NewFloorPlanReservation("code-977", "label-708");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FloorPlanReservationEngineKeepsResultsWithinBounds();
    }
}
