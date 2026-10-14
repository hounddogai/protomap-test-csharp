namespace Acme.Workforce.Payrollops.Tests.RetroPay.PaySummary;

using Acme.Workforce.Payrollops.RetroPay.PaySummary;

/// <summary>Checks of pay summary code, run by the project's runner.</summary>
public static class LenientRetroPaySummaryEngineChecks
{
    public static void LenientRetroPaySummaryEngineKeepsResultsWithinBounds()
    {
        var engine = LenientRetroPaySummaryEngine.DefaultLenientRetroPaySummaryEngine();
        var item = LenientRetroPaySummary.NewLenientRetroPaySummary("code-387", "name-362");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientRetroPaySummaryEngineKeepsResultsWithinBounds();
    }
}
