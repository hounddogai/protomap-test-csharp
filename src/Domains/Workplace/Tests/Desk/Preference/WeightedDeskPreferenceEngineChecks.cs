namespace Acme.Workforce.Workplace.Tests.Desk.Preference;

using Acme.Workforce.Workplace.Desk.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class WeightedDeskPreferenceEngineChecks
{
    public static void WeightedDeskPreferenceEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedDeskPreferenceEngine.DefaultWeightedDeskPreferenceEngine();
        var item = WeightedDeskPreference.NewWeightedDeskPreference("code-217", "label-343");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedDeskPreferenceEngineKeepsResultsWithinBounds();
    }
}
