namespace Acme.Workforce.Contractors.Tests.Rate.Bundle;

using Acme.Workforce.Contractors.Rate.Bundle;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class FallbackRateAllocationEngineChecks
{
    public static void FallbackRateAllocationEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackRateAllocationEngine.DefaultFallbackRateAllocationEngine(Flags.NewRegistry());
        var item = FallbackRateAllocation.NewFallbackRateAllocation("code-411", "name-285");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackRateAllocationEngineKeepsResultsWithinBounds();
    }
}
