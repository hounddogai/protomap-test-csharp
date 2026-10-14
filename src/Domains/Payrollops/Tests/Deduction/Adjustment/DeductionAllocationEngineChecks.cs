namespace Acme.Workforce.Payrollops.Tests.Deduction.Adjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Deduction.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class DeductionAllocationEngineChecks
{
    public static void DeductionAllocationEngineKeepsResultsWithinBounds()
    {
        var engine = DeductionAllocationEngine.DefaultDeductionAllocationEngine(Flags.NewRegistry());
        var item = DeductionAllocation.NewDeductionAllocation("code-631", "title-899");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DeductionAllocationEngineKeepsResultsWithinBounds();
    }
}
