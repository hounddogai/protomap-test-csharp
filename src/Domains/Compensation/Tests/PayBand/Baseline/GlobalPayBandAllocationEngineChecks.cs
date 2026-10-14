namespace Acme.Workforce.Compensation.Tests.PayBand.Baseline;

using Acme.Workforce.Compensation.PayBand.Baseline;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class GlobalPayBandAllocationEngineChecks
{
    public static void GlobalPayBandAllocationEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalPayBandAllocationEngine.DefaultGlobalPayBandAllocationEngine(Flags.NewRegistry());
        var item = GlobalPayBandAllocation.NewGlobalPayBandAllocation("code-682", "title-244");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalPayBandAllocationEngineKeepsResultsWithinBounds();
    }
}
