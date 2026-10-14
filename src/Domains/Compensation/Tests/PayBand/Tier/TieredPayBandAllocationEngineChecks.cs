namespace Acme.Workforce.Compensation.Tests.PayBand.Tier;

using Acme.Workforce.Compensation.PayBand.Tier;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class TieredPayBandAllocationEngineChecks
{
    public static void TieredPayBandAllocationEngineKeepsResultsWithinBounds()
    {
        var engine = TieredPayBandAllocationEngine.DefaultTieredPayBandAllocationEngine(Flags.NewRegistry());
        var item = TieredPayBandAllocation.NewTieredPayBandAllocation("code-198", "name-59");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredPayBandAllocationEngineKeepsResultsWithinBounds();
    }
}
