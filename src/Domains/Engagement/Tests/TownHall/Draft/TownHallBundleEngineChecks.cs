namespace Acme.Workforce.Engagement.Tests.TownHall.Draft;

using Acme.Workforce.Engagement.TownHall.Draft;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class TownHallBundleEngineChecks
{
    public static void TownHallBundleEngineKeepsResultsWithinBounds()
    {
        var engine = TownHallBundleEngine.DefaultTownHallBundleEngine(Flags.NewRegistry());
        var item = TownHallBundle.NewTownHallBundle("code-452", "name-546");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TownHallBundleEngineKeepsResultsWithinBounds();
    }
}
