namespace Acme.Workforce.Payrollops.Tests.Garnishment.Allocation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Garnishment.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class GarnishmentAllocationEngineChecks
{
    public static void GarnishmentAllocationEngineKeepsResultsWithinBounds()
    {
        var engine = GarnishmentAllocationEngine.DefaultGarnishmentAllocationEngine(Flags.NewRegistry());
        var item = GarnishmentAllocation.NewGarnishmentAllocation("code-196", "title-532");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GarnishmentAllocationEngineKeepsResultsWithinBounds();
    }
}
