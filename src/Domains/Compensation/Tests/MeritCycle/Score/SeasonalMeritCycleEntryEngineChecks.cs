namespace Acme.Workforce.Compensation.Tests.MeritCycle.Score;

using Acme.Workforce.Compensation.MeritCycle.Score;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class SeasonalMeritCycleEntryEngineChecks
{
    public static void SeasonalMeritCycleEntryEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalMeritCycleEntryEngine.DefaultSeasonalMeritCycleEntryEngine(Flags.NewRegistry());
        var item = SeasonalMeritCycleEntry.NewSeasonalMeritCycleEntry("code-217", "label-366");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalMeritCycleEntryEngineKeepsResultsWithinBounds();
    }
}
