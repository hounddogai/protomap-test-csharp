namespace Acme.Workforce.Orgdesign.Tests.Scenario.Request;

using Acme.Workforce.Orgdesign.Scenario.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class FallbackScenarioOverrideEngineChecks
{
    public static void FallbackScenarioOverrideEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackScenarioOverrideEngine.DefaultFallbackScenarioOverrideEngine();
        var item = FallbackScenarioOverride.NewFallbackScenarioOverride("code-472", "name-285");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackScenarioOverrideEngineKeepsResultsWithinBounds();
    }
}
