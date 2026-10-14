namespace Acme.Workforce.Orgdesign.Tests.Headcount.Allocation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Headcount.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class HeadcountAllocationEngineChecks
{
    public static void HeadcountAllocationEngineKeepsResultsWithinBounds()
    {
        var engine = HeadcountAllocationEngine.DefaultHeadcountAllocationEngine(Flags.NewRegistry());
        var item = HeadcountAllocation.NewHeadcountAllocation("code-921", "label-17");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        HeadcountAllocationEngineKeepsResultsWithinBounds();
    }
}
