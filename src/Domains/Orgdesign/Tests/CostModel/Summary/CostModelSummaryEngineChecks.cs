namespace Acme.Workforce.Orgdesign.Tests.CostModel.Summary;

using Acme.Workforce.Orgdesign.CostModel.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class CostModelSummaryEngineChecks
{
    public static void CostModelSummaryEngineKeepsResultsWithinBounds()
    {
        var engine = CostModelSummaryEngine.DefaultCostModelSummaryEngine();
        var item = CostModelSummary.NewCostModelSummary("code-497", "label-626");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CostModelSummaryEngineKeepsResultsWithinBounds();
    }
}
