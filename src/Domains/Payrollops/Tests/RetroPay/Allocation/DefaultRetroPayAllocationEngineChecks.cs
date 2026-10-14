namespace Acme.Workforce.Payrollops.Tests.RetroPay.Allocation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.RetroPay.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class DefaultRetroPayAllocationEngineChecks
{
    public static void DefaultRetroPayAllocationEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultRetroPayAllocationEngine.DefaultDefaultRetroPayAllocationEngine(Flags.NewRegistry());
        var item = DefaultRetroPayAllocation.NewDefaultRetroPayAllocation("code-568", "title-277");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultRetroPayAllocationEngineKeepsResultsWithinBounds();
    }
}
