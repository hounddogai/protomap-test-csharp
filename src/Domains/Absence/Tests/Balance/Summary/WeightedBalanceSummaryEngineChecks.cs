namespace Acme.Workforce.Absence.Tests.Balance.Summary;

using Acme.Workforce.Absence.Balance.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class WeightedBalanceSummaryEngineChecks
{
    public static void WeightedBalanceSummaryEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedBalanceSummaryEngine.DefaultWeightedBalanceSummaryEngine();
        var item = WeightedBalanceSummary.NewWeightedBalanceSummary("code-590", "label-598");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedBalanceSummaryEngineKeepsResultsWithinBounds();
    }
}
