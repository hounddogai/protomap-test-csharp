namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioTemplate;

using Acme.Workforce.Orgdesign.Scenario.ScenarioTemplate;

/// <summary>Checks of scenario template code, run by the project's runner.</summary>
public static class ManualScenarioAssignmentEngineChecks
{
    public static void ManualScenarioAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = ManualScenarioAssignmentEngine.DefaultManualScenarioAssignmentEngine();
        var item = ManualScenarioAssignment.NewManualScenarioAssignment("code-442", "title-539");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ManualScenarioAssignmentEngineKeepsResultsWithinBounds();
    }
}
