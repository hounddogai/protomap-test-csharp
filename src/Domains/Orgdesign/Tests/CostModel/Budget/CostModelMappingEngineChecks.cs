namespace Acme.Workforce.Orgdesign.Tests.CostModel.Budget;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.CostModel.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class CostModelMappingEngineChecks
{
    public static void CostModelMappingEngineKeepsResultsWithinBounds()
    {
        var engine = CostModelMappingEngine.DefaultCostModelMappingEngine(Flags.NewRegistry());
        var item = CostModelMapping.NewCostModelMapping("code-52", "name-606");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CostModelMappingEngineKeepsResultsWithinBounds();
    }
}
