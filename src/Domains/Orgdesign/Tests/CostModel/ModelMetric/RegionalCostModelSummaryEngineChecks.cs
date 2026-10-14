namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelMetric;

using Acme.Workforce.Orgdesign.CostModel.ModelMetric;

/// <summary>Checks of model metric code, run by the project's runner.</summary>
public static class RegionalCostModelSummaryEngineChecks
{
    public static void RegionalCostModelSummaryEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalCostModelSummaryEngine.DefaultRegionalCostModelSummaryEngine();
        var item = RegionalCostModelSummary.NewRegionalCostModelSummary("code-661", "title-450");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalCostModelSummaryEngineKeepsResultsWithinBounds();
    }
}
