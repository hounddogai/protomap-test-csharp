namespace Acme.Workforce.Talent.Tests.Mobility.Preference;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Mobility.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class SeasonalMobilityManifestEngineChecks
{
    public static void SeasonalMobilityManifestEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalMobilityManifestEngine.DefaultSeasonalMobilityManifestEngine(Flags.NewRegistry());
        var item = SeasonalMobilityManifest.NewSeasonalMobilityManifest("code-112", "name-471");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalMobilityManifestEngineKeepsResultsWithinBounds();
    }
}
