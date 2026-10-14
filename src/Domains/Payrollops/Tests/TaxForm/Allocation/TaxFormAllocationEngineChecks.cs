namespace Acme.Workforce.Payrollops.Tests.TaxForm.Allocation;

using Acme.Workforce.Payrollops.TaxForm.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class TaxFormAllocationEngineChecks
{
    public static void TaxFormAllocationEngineKeepsResultsWithinBounds()
    {
        var engine = TaxFormAllocationEngine.DefaultTaxFormAllocationEngine();
        var item = TaxFormAllocation.NewTaxFormAllocation("code-400", "label-623");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TaxFormAllocationEngineKeepsResultsWithinBounds();
    }
}
