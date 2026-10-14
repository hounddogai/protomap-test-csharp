namespace Acme.Workforce.Engagement.Tests.TownHall.Entry;

using Acme.Workforce.Engagement.TownHall.Entry;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class TownHallEntryEngineChecks
{
    public static void TownHallEntryEngineKeepsResultsWithinBounds()
    {
        var engine = TownHallEntryEngine.DefaultTownHallEntryEngine(Flags.NewRegistry());
        var item = TownHallEntry.NewTownHallEntry("code-982", "name-465");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TownHallEntryEngineKeepsResultsWithinBounds();
    }
}
