namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioPolicy;

using Acme.Workforce.Orgdesign.Scenario.ScenarioPolicy;

/// <summary>Checks of scenario policy code, run by the project's runner.</summary>
public static class ScenarioAgreementEngineChecks
{
    public static void ScenarioAgreementEngineKeepsResultsWithinBounds()
    {
        var engine = ScenarioAgreementEngine.DefaultScenarioAgreementEngine();
        var item = ScenarioAgreement.NewScenarioAgreement("code-747", "name-426");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScenarioAgreementEngineKeepsResultsWithinBounds();
    }
}
