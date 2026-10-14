namespace Acme.Workforce.Payrollops.Tests.Payslip.Manifest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Payslip.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class PayslipEntryEngineChecks
{
    public static void PayslipEntryEngineKeepsResultsWithinBounds()
    {
        var engine = PayslipEntryEngine.DefaultPayslipEntryEngine(Flags.NewRegistry());
        var item = PayslipEntry.NewPayslipEntry("code-723", "title-119");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PayslipEntryEngineKeepsResultsWithinBounds();
    }
}
