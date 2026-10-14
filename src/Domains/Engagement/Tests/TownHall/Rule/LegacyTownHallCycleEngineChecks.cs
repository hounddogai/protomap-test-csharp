namespace Acme.Workforce.Engagement.Tests.TownHall.Rule;

using Acme.Workforce.Engagement.TownHall.Rule;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class LegacyTownHallCycleEngineChecks
{
    public static void LegacyTownHallCycleEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyTownHallCycleEngine.DefaultLegacyTownHallCycleEngine(Flags.NewRegistry());
        var item = LegacyTownHallCycle.NewLegacyTownHallCycle("code-873", "title-319");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyTownHallCycleEngineKeepsResultsWithinBounds();
    }
}
