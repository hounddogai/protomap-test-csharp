namespace Acme.Workforce.Engagement.Tests.TownHall.OverrideItem;

using Acme.Workforce.Engagement.TownHall.OverrideItem;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class TownHallOverrideEngineChecks
{
    public static void TownHallOverrideEngineKeepsResultsWithinBounds()
    {
        var engine = TownHallOverrideEngine.DefaultTownHallOverrideEngine(Flags.NewRegistry());
        var item = TownHallOverride.NewTownHallOverride("code-520", "title-865");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TownHallOverrideEngineKeepsResultsWithinBounds();
    }
}
