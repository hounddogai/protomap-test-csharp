namespace Acme.Workforce.Absence.Tests.Accrual.Batch;

using Acme.Workforce.Absence.Accrual.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class AccrualEstimateEngineChecks
{
    public static void AccrualEstimateEngineKeepsResultsWithinBounds()
    {
        var engine = AccrualEstimateEngine.DefaultAccrualEstimateEngine();
        var item = AccrualEstimate.NewAccrualEstimate("code-344", "name-374");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AccrualEstimateEngineKeepsResultsWithinBounds();
    }
}
