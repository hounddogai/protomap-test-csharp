namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioRule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Scenario.ScenarioRule;

/// <summary>Checks of scenario rule code, run by the project's runner.</summary>
public static class ScheduledScenarioManifestEngineChecks
{
    public static void ScheduledScenarioManifestEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledScenarioManifestEngine.DefaultScheduledScenarioManifestEngine(Flags.NewRegistry());
        var item = ScheduledScenarioManifest.NewScheduledScenarioManifest("code-75", "name-699");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledScenarioManifestEngineKeepsResultsWithinBounds();
    }
}
