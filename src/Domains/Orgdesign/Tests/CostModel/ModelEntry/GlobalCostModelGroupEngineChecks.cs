namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelEntry;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.CostModel.ModelEntry;

/// <summary>Checks of model entry code, run by the project's runner.</summary>
public static class GlobalCostModelGroupEngineChecks
{
    public static void GlobalCostModelGroupEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalCostModelGroupEngine.DefaultGlobalCostModelGroupEngine(Flags.NewRegistry());
        var item = GlobalCostModelGroup.NewGlobalCostModelGroup("code-71", "name-271");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalCostModelGroupEngineKeepsResultsWithinBounds();
    }
}
