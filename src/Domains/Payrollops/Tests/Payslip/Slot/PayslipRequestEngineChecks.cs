namespace Acme.Workforce.Payrollops.Tests.Payslip.Slot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Payslip.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class PayslipRequestEngineChecks
{
    public static void PayslipRequestEngineKeepsResultsWithinBounds()
    {
        var engine = PayslipRequestEngine.DefaultPayslipRequestEngine(Flags.NewRegistry());
        var item = PayslipRequest.NewPayslipRequest("code-917", "title-894");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PayslipRequestEngineKeepsResultsWithinBounds();
    }
}
