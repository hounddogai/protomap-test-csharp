namespace Acme.Workforce.Engagement.Tests.TownHall.Milestone;

using Acme.Workforce.Engagement.TownHall.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class TownHallScheduleEngineChecks
{
    public static void TownHallScheduleEngineKeepsResultsWithinBounds()
    {
        var engine = TownHallScheduleEngine.DefaultTownHallScheduleEngine();
        var item = TownHallSchedule.NewTownHallSchedule("code-171", "label-781");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TownHallScheduleEngineKeepsResultsWithinBounds();
    }
}
