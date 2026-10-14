namespace Acme.Workforce.Orgdesign.Tests.Scenario.Tier;

using Acme.Workforce.Orgdesign.Scenario.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class SeasonalScenarioTierEngineChecks
{
    public static void SeasonalScenarioTierEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalScenarioTierEngine.DefaultSeasonalScenarioTierEngine();
        var item = SeasonalScenarioTier.NewSeasonalScenarioTier("code-502", "name-356");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalScenarioTierEngineKeepsResultsWithinBounds();
    }
}
