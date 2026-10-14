namespace Acme.Workforce.Engagement.Tests.TownHall.BatchedHallRule;

using Acme.Workforce.Engagement.TownHall.BatchedHallRule;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of batched hall rule code, run by the project's runner.</summary>
public static class TownHallGroupEngineChecks
{
    public static void TownHallGroupEngineKeepsResultsWithinBounds()
    {
        var engine = TownHallGroupEngine.DefaultTownHallGroupEngine(Flags.NewRegistry());
        var item = TownHallGroup.NewTownHallGroup("code-346", "label-873");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TownHallGroupEngineKeepsResultsWithinBounds();
    }
}
