namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelPreference;

using Acme.Workforce.Orgdesign.CostModel.ModelPreference;

/// <summary>Checks of model preference code, run by the project's runner.</summary>
public static class RollingCostModelPreferenceEngineChecks
{
    public static void RollingCostModelPreferenceEngineKeepsResultsWithinBounds()
    {
        var engine = RollingCostModelPreferenceEngine.DefaultRollingCostModelPreferenceEngine();
        var item = RollingCostModelPreference.NewRollingCostModelPreference("code-294", "title-981");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RollingCostModelPreferenceEngineKeepsResultsWithinBounds();
    }
}
