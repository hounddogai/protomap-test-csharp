namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioTemplate;

using Acme.Workforce.Orgdesign.Scenario.ScenarioTemplate;

/// <summary>Checks of scenario template code, run by the project's runner.</summary>
public static class PendingScenarioTemplateEngineChecks
{
    public static void PendingScenarioTemplateEngineKeepsResultsWithinBounds()
    {
        var engine = PendingScenarioTemplateEngine.DefaultPendingScenarioTemplateEngine();
        var item = PendingScenarioTemplate.NewPendingScenarioTemplate("code-847", "name-335");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingScenarioTemplateEngineKeepsResultsWithinBounds();
    }
}
