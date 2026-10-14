namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioAudit;

using Acme.Workforce.Orgdesign.Scenario.ScenarioAudit;

/// <summary>Checks of scenario audit code, run by the project's runner.</summary>
public static class PrimaryScenarioTierEngineChecks
{
    public static void PrimaryScenarioTierEngineKeepsResultsWithinBounds()
    {
        var engine = PrimaryScenarioTierEngine.DefaultPrimaryScenarioTierEngine();
        var item = PrimaryScenarioTier.NewPrimaryScenarioTier("code-842", "title-255");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PrimaryScenarioTierEngineKeepsResultsWithinBounds();
    }
}
