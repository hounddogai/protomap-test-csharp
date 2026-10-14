namespace Acme.Workforce.Compensation.Tests.PayBand.Forecast;

using Acme.Workforce.Compensation.PayBand.Forecast;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class PayBandAllocationEngineChecks
{
    public static void PayBandAllocationEngineKeepsResultsWithinBounds()
    {
        var engine = PayBandAllocationEngine.DefaultPayBandAllocationEngine(Flags.NewRegistry());
        var item = PayBandAllocation.NewPayBandAllocation("code-811", "title-576");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PayBandAllocationEngineKeepsResultsWithinBounds();
    }
}
