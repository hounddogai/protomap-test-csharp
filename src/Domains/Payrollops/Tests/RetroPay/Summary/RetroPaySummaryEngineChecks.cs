namespace Acme.Workforce.Payrollops.Tests.RetroPay.Summary;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.RetroPay.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class RetroPaySummaryEngineChecks
{
    public static void RetroPaySummaryEngineKeepsResultsWithinBounds()
    {
        var engine = RetroPaySummaryEngine.DefaultRetroPaySummaryEngine(Flags.NewRegistry());
        var item = RetroPaySummary.NewRetroPaySummary("code-852", "label-960");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RetroPaySummaryEngineKeepsResultsWithinBounds();
    }
}
