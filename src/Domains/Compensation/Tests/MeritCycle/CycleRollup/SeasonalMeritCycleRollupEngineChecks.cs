namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleRollup;

using Acme.Workforce.Compensation.MeritCycle.CycleRollup;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of cycle rollup code, run by the project's runner.</summary>
public static class SeasonalMeritCycleRollupEngineChecks
{
    public static void SeasonalMeritCycleRollupEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalMeritCycleRollupEngine.DefaultSeasonalMeritCycleRollupEngine(Flags.NewRegistry());
        var item = SeasonalMeritCycleRollup.NewSeasonalMeritCycleRollup("code-324", "title-818");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalMeritCycleRollupEngineKeepsResultsWithinBounds();
    }
}
