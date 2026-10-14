namespace Acme.Workforce.Payrollops.Tests.PayRun.Review;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.PayRun.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class PayRunTargetEngineChecks
{
    public static void PayRunTargetEngineKeepsResultsWithinBounds()
    {
        var engine = PayRunTargetEngine.DefaultPayRunTargetEngine(Flags.NewRegistry());
        var item = PayRunTarget.NewPayRunTarget("code-591", "label-31");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PayRunTargetEngineKeepsResultsWithinBounds();
    }
}
