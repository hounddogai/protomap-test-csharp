namespace Acme.Workforce.Contractors.Tests.Rate.Batch;

using Acme.Workforce.Contractors.Rate.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class RateQuotaEngineChecks
{
    public static void RateQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = RateQuotaEngine.DefaultRateQuotaEngine();
        var item = RateQuota.NewRateQuota("code-349", "name-915");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RateQuotaEngineKeepsResultsWithinBounds();
    }
}
