namespace Acme.Workforce.Orgdesign.Tests.Reorg.Schedule;

using Acme.Workforce.Orgdesign.Reorg.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class ReorgScheduleEngineChecks
{
    public static void ReorgScheduleEngineKeepsResultsWithinBounds()
    {
        var engine = ReorgScheduleEngine.DefaultReorgScheduleEngine();
        var item = ReorgSchedule.NewReorgSchedule("code-407", "label-415");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ReorgScheduleEngineKeepsResultsWithinBounds();
    }
}
