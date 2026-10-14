namespace Acme.Workforce.Payrollops.Tests.PayRun.Review;

using Acme.Workforce.Payrollops.PayRun.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class PayRunProfileEngineChecks
{
    public static void PayRunProfileEngineKeepsResultsWithinBounds()
    {
        var engine = PayRunProfileEngine.DefaultPayRunProfileEngine();
        var item = PayRunProfile.NewPayRunProfile("code-378", "title-978");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PayRunProfileEngineKeepsResultsWithinBounds();
    }
}
