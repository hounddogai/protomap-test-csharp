namespace Acme.Workforce.Orgdesign.Tests.Headcount.Schedule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Headcount.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class WeightedHeadcountScheduleEngineChecks
{
    public static void WeightedHeadcountScheduleEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedHeadcountScheduleEngine.DefaultWeightedHeadcountScheduleEngine(Flags.NewRegistry());
        var item = WeightedHeadcountSchedule.NewWeightedHeadcountSchedule("code-526", "name-831");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedHeadcountScheduleEngineKeepsResultsWithinBounds();
    }
}
