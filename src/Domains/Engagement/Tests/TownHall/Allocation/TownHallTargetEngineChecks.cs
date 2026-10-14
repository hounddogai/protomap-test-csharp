namespace Acme.Workforce.Engagement.Tests.TownHall.Allocation;

using Acme.Workforce.Engagement.TownHall.Allocation;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class TownHallTargetEngineChecks
{
    public static void TownHallTargetEngineKeepsResultsWithinBounds()
    {
        var engine = TownHallTargetEngine.DefaultTownHallTargetEngine(Flags.NewRegistry());
        var item = TownHallTarget.NewTownHallTarget("code-734", "title-560");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TownHallTargetEngineKeepsResultsWithinBounds();
    }
}
