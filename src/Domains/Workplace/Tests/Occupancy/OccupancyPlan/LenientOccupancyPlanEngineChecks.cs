namespace Acme.Workforce.Workplace.Tests.Occupancy.OccupancyPlan;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Occupancy.OccupancyPlan;

/// <summary>Checks of occupancy plan code, run by the project's runner.</summary>
public static class LenientOccupancyPlanEngineChecks
{
    public static void LenientOccupancyPlanEngineKeepsResultsWithinBounds()
    {
        var engine = LenientOccupancyPlanEngine.DefaultLenientOccupancyPlanEngine(Flags.NewRegistry());
        var item = LenientOccupancyPlan.NewLenientOccupancyPlan("code-479", "title-745");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientOccupancyPlanEngineKeepsResultsWithinBounds();
    }
}
