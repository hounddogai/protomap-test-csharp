namespace Acme.Workforce.Payrollops.Tests.PayRun.Bundle;

using Acme.Workforce.Payrollops.PayRun.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class PayRunSummaryEngineChecks
{
    public static void PayRunSummaryEngineKeepsResultsWithinBounds()
    {
        var engine = PayRunSummaryEngine.DefaultPayRunSummaryEngine();
        var item = PayRunSummary.NewPayRunSummary("code-656", "title-91");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PayRunSummaryEngineKeepsResultsWithinBounds();
    }
}
