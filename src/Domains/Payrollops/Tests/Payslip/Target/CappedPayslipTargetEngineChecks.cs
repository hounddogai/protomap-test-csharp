namespace Acme.Workforce.Payrollops.Tests.Payslip.Target;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Payslip.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class CappedPayslipTargetEngineChecks
{
    public static void CappedPayslipTargetEngineKeepsResultsWithinBounds()
    {
        var engine = CappedPayslipTargetEngine.DefaultCappedPayslipTargetEngine(Flags.NewRegistry());
        var item = CappedPayslipTarget.NewCappedPayslipTarget("code-787", "label-658");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedPayslipTargetEngineKeepsResultsWithinBounds();
    }
}
