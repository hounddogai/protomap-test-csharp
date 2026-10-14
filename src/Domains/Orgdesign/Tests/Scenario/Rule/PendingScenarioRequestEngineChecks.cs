namespace Acme.Workforce.Orgdesign.Tests.Scenario.Rule;

using Acme.Workforce.Orgdesign.Scenario.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class PendingScenarioRequestEngineChecks
{
    public static void PendingScenarioRequestEngineKeepsResultsWithinBounds()
    {
        var engine = PendingScenarioRequestEngine.DefaultPendingScenarioRequestEngine();
        var item = PendingScenarioRequest.NewPendingScenarioRequest("code-596", "label-705");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingScenarioRequestEngineKeepsResultsWithinBounds();
    }
}
