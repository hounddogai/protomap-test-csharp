namespace Acme.Workforce.Workplace.Tests.FloorPlan.Reservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.FloorPlan.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class FloorPlanScheduleEngineChecks
{
    public static void FloorPlanScheduleEngineKeepsResultsWithinBounds()
    {
        var engine = FloorPlanScheduleEngine.DefaultFloorPlanScheduleEngine(Flags.NewRegistry());
        var item = FloorPlanSchedule.NewFloorPlanSchedule("code-891", "name-249");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FloorPlanScheduleEngineKeepsResultsWithinBounds();
    }
}
