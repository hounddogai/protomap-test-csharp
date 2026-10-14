namespace Acme.Workforce.Orgdesign.Tests.CostModel.Policy;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.CostModel.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class CostModelExceptionEngineChecks
{
    public static void CostModelExceptionEngineKeepsResultsWithinBounds()
    {
        var engine = CostModelExceptionEngine.DefaultCostModelExceptionEngine(Flags.NewRegistry());
        var item = CostModelException.NewCostModelException("code-565", "name-417");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CostModelExceptionEngineKeepsResultsWithinBounds();
    }
}
