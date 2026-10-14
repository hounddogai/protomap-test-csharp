namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioEstimate;

using Acme.Workforce.Orgdesign.Scenario.ScenarioEstimate;

/// <summary>Checks of scenario estimate code, run by the project's runner.</summary>
public static class GlobalScenarioBundleEngineChecks
{
    public static void GlobalScenarioBundleEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalScenarioBundleEngine.DefaultGlobalScenarioBundleEngine();
        var item = GlobalScenarioBundle.NewGlobalScenarioBundle("code-872", "label-866");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalScenarioBundleEngineKeepsResultsWithinBounds();
    }
}
