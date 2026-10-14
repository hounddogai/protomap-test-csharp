namespace Acme.Workforce.Workplace.Tests.Occupancy.Preference;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Occupancy.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class OccupancyPreferenceEngineChecks
{
    public static void OccupancyPreferenceEngineKeepsResultsWithinBounds()
    {
        var engine = OccupancyPreferenceEngine.DefaultOccupancyPreferenceEngine(Flags.NewRegistry());
        var item = OccupancyPreference.NewOccupancyPreference("code-487", "title-703");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        OccupancyPreferenceEngineKeepsResultsWithinBounds();
    }
}
