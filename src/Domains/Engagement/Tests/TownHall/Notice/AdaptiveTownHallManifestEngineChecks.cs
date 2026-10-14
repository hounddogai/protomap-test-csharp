namespace Acme.Workforce.Engagement.Tests.TownHall.Notice;

using Acme.Workforce.Engagement.TownHall.Notice;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class AdaptiveTownHallManifestEngineChecks
{
    public static void AdaptiveTownHallManifestEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveTownHallManifestEngine.DefaultAdaptiveTownHallManifestEngine(Flags.NewRegistry());
        var item = AdaptiveTownHallManifest.NewAdaptiveTownHallManifest("code-25", "name-668");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveTownHallManifestEngineKeepsResultsWithinBounds();
    }
}
