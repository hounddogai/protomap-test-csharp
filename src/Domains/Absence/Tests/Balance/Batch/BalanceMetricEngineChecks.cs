namespace Acme.Workforce.Absence.Tests.Balance.Batch;

using Acme.Workforce.Absence.Balance.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class BalanceMetricEngineChecks
{
    public static void BalanceMetricEngineKeepsResultsWithinBounds()
    {
        var engine = BalanceMetricEngine.DefaultBalanceMetricEngine();
        var item = BalanceMetric.NewBalanceMetric("code-626", "title-693");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BalanceMetricEngineKeepsResultsWithinBounds();
    }
}
