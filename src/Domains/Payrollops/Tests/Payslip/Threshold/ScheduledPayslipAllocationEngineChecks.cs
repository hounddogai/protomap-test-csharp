namespace Acme.Workforce.Payrollops.Tests.Payslip.Threshold;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Payslip.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class ScheduledPayslipAllocationEngineChecks
{
    public static void ScheduledPayslipAllocationEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledPayslipAllocationEngine.DefaultScheduledPayslipAllocationEngine(Flags.NewRegistry());
        var item = ScheduledPayslipAllocation.NewScheduledPayslipAllocation("code-583", "label-712");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledPayslipAllocationEngineKeepsResultsWithinBounds();
    }
}
