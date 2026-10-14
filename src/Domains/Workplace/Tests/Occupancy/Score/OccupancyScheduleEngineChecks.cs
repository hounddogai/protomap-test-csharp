namespace Acme.Workforce.Workplace.Tests.Occupancy.Score;

using Acme.Workforce.Workplace.Occupancy.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class OccupancyScheduleEngineChecks
{
    public static void OccupancyScheduleEngineKeepsResultsWithinBounds()
    {
        var engine = OccupancyScheduleEngine.DefaultOccupancyScheduleEngine();
        var item = OccupancySchedule.NewOccupancySchedule("code-646", "title-947");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        OccupancyScheduleEngineKeepsResultsWithinBounds();
    }
}
