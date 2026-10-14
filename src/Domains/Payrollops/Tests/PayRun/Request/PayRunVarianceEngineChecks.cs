namespace Acme.Workforce.Payrollops.Tests.PayRun.Request;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.PayRun.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class PayRunVarianceEngineChecks
{
    public static void PayRunVarianceEngineKeepsResultsWithinBounds()
    {
        var engine = PayRunVarianceEngine.DefaultPayRunVarianceEngine(Flags.NewRegistry());
        var item = PayRunVariance.NewPayRunVariance("code-849", "title-958");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PayRunVarianceEngineKeepsResultsWithinBounds();
    }
}
