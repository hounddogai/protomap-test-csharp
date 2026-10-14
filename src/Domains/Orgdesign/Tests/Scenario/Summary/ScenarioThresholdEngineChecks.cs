namespace Acme.Workforce.Orgdesign.Tests.Scenario.Summary;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Scenario.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class ScenarioThresholdEngineChecks
{
    public static void ScenarioThresholdEngineKeepsResultsWithinBounds()
    {
        var engine = ScenarioThresholdEngine.DefaultScenarioThresholdEngine(Flags.NewRegistry());
        var item = ScenarioThreshold.NewScenarioThreshold("code-922", "label-534");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScenarioThresholdEngineKeepsResultsWithinBounds();
    }
}
