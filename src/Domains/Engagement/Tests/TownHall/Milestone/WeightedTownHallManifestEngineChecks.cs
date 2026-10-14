namespace Acme.Workforce.Engagement.Tests.TownHall.Milestone;

using Acme.Workforce.Engagement.TownHall.Milestone;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class WeightedTownHallManifestEngineChecks
{
    public static void WeightedTownHallManifestEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedTownHallManifestEngine.DefaultWeightedTownHallManifestEngine(Flags.NewRegistry());
        var item = WeightedTownHallManifest.NewWeightedTownHallManifest("code-746", "name-118");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedTownHallManifestEngineKeepsResultsWithinBounds();
    }
}
