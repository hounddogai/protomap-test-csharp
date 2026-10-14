namespace Acme.Workforce.Payrollops.Tests.TaxForm.Slot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.TaxForm.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class TaxFormSlotEngineChecks
{
    public static void TaxFormSlotEngineKeepsResultsWithinBounds()
    {
        var engine = TaxFormSlotEngine.DefaultTaxFormSlotEngine(Flags.NewRegistry());
        var item = TaxFormSlot.NewTaxFormSlot("code-546", "title-167");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TaxFormSlotEngineKeepsResultsWithinBounds();
    }
}
