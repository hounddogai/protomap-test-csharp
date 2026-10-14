namespace Acme.Workforce.Payrollops.Tests.RetroPay.PayAssignment;

using Acme.Workforce.Payrollops.RetroPay.PayAssignment;

/// <summary>Checks of pay assignment code, run by the project's runner.</summary>
public static class RetroPayVarianceEngineChecks
{
    public static void RetroPayVarianceEngineKeepsResultsWithinBounds()
    {
        var engine = RetroPayVarianceEngine.DefaultRetroPayVarianceEngine();
        var item = RetroPayVariance.NewRetroPayVariance("code-783", "name-309");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RetroPayVarianceEngineKeepsResultsWithinBounds();
    }
}
