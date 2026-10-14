namespace Acme.Workforce.Payrollops.Tests.Payslip.Slot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Payslip.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class PayslipSlotEngineChecks
{
    public static void PayslipSlotEngineKeepsResultsWithinBounds()
    {
        var engine = PayslipSlotEngine.DefaultPayslipSlotEngine(Flags.NewRegistry());
        var item = PayslipSlot.NewPayslipSlot("code-750", "title-118");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PayslipSlotEngineKeepsResultsWithinBounds();
    }
}
