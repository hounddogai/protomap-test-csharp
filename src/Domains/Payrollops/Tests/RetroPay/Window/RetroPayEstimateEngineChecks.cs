namespace Acme.Workforce.Payrollops.Tests.RetroPay.Window;

using Acme.Workforce.Payrollops.RetroPay.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class RetroPayEstimateEngineChecks
{
    public static void RetroPayEstimateEngineKeepsResultsWithinBounds()
    {
        var engine = RetroPayEstimateEngine.DefaultRetroPayEstimateEngine();
        var item = RetroPayEstimate.NewRetroPayEstimate("code-470", "title-326");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RetroPayEstimateEngineKeepsResultsWithinBounds();
    }
}
