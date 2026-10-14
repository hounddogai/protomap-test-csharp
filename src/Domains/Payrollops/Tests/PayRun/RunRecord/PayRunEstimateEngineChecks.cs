namespace Acme.Workforce.Payrollops.Tests.PayRun.RunRecord;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.PayRun.RunRecord;

/// <summary>Checks of run record code, run by the project's runner.</summary>
public static class PayRunEstimateEngineChecks
{
    public static void PayRunEstimateEngineKeepsResultsWithinBounds()
    {
        var engine = PayRunEstimateEngine.DefaultPayRunEstimateEngine(Flags.NewRegistry());
        var item = PayRunEstimate.NewPayRunEstimate("code-930", "name-550");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PayRunEstimateEngineKeepsResultsWithinBounds();
    }
}
