namespace Acme.Workforce.Orgdesign.Tests.Scenario.Estimate;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Scenario.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class ScenarioBudgetEngineChecks
{
    public static void ScenarioBudgetEngineKeepsResultsWithinBounds()
    {
        var engine = ScenarioBudgetEngine.DefaultScenarioBudgetEngine(Flags.NewRegistry());
        var item = ScenarioBudget.NewScenarioBudget("code-279", "label-139");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScenarioBudgetEngineKeepsResultsWithinBounds();
    }
}
