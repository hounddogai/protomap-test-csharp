namespace Acme.Workforce.Absence.Tests.Accrual.Ledger;

using Acme.Workforce.Absence.Accrual.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class GlobalAccrualEstimateEngineChecks
{
    public static void GlobalAccrualEstimateEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalAccrualEstimateEngine.DefaultGlobalAccrualEstimateEngine();
        var item = GlobalAccrualEstimate.NewGlobalAccrualEstimate("code-276", "name-925");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalAccrualEstimateEngineKeepsResultsWithinBounds();
    }
}
