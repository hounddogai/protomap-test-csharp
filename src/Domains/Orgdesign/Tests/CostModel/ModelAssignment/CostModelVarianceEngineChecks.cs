namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelAssignment;

using Acme.Workforce.Orgdesign.CostModel.ModelAssignment;

/// <summary>Checks of model assignment code, run by the project's runner.</summary>
public static class CostModelVarianceEngineChecks
{
    public static void CostModelVarianceEngineKeepsResultsWithinBounds()
    {
        var engine = CostModelVarianceEngine.DefaultCostModelVarianceEngine();
        var item = CostModelVariance.NewCostModelVariance("code-213", "name-552");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CostModelVarianceEngineKeepsResultsWithinBounds();
    }
}
