namespace Acme.Workforce.Orgdesign.Tests.CostModel.Preference;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.CostModel.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class CostModelGroupEngineChecks
{
    public static void CostModelGroupEngineKeepsResultsWithinBounds()
    {
        var engine = CostModelGroupEngine.DefaultCostModelGroupEngine(Flags.NewRegistry());
        var item = CostModelGroup.NewCostModelGroup("code-404", "name-897");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CostModelGroupEngineKeepsResultsWithinBounds();
    }
}
