namespace Acme.Workforce.Orgdesign.Tests.Scenario.Quota;

using Acme.Workforce.Orgdesign.Scenario.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class ScenarioQuotaEngineChecks
{
    public static void ScenarioQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = ScenarioQuotaEngine.DefaultScenarioQuotaEngine();
        var item = ScenarioQuota.NewScenarioQuota("code-158", "title-116");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScenarioQuotaEngineKeepsResultsWithinBounds();
    }
}
