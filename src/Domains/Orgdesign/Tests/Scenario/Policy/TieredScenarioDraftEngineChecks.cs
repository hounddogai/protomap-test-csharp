namespace Acme.Workforce.Orgdesign.Tests.Scenario.Policy;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Scenario.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class TieredScenarioDraftEngineChecks
{
    public static void TieredScenarioDraftEngineKeepsResultsWithinBounds()
    {
        var engine = TieredScenarioDraftEngine.DefaultTieredScenarioDraftEngine(Flags.NewRegistry());
        var item = TieredScenarioDraft.NewTieredScenarioDraft("code-290", "label-834");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredScenarioDraftEngineKeepsResultsWithinBounds();
    }
}
