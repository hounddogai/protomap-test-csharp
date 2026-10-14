namespace Acme.Workforce.Orgdesign.Tests.CostModel.Preference;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.CostModel.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class SeasonalCostModelPreferenceEngineChecks
{
    public static void SeasonalCostModelPreferenceEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalCostModelPreferenceEngine.DefaultSeasonalCostModelPreferenceEngine(Flags.NewRegistry());
        var item = SeasonalCostModelPreference.NewSeasonalCostModelPreference("code-88", "name-728");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalCostModelPreferenceEngineKeepsResultsWithinBounds();
    }
}
