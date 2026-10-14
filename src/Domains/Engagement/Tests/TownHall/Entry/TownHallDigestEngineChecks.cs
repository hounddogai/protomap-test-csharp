namespace Acme.Workforce.Engagement.Tests.TownHall.Entry;

using Acme.Workforce.Engagement.TownHall.Entry;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class TownHallDigestEngineChecks
{
    public static void TownHallDigestEngineKeepsResultsWithinBounds()
    {
        var engine = TownHallDigestEngine.DefaultTownHallDigestEngine(Flags.NewRegistry());
        var item = TownHallDigest.NewTownHallDigest("code-57", "label-655");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TownHallDigestEngineKeepsResultsWithinBounds();
    }
}
