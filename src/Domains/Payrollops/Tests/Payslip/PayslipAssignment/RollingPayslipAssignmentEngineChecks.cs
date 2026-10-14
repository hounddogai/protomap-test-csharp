namespace Acme.Workforce.Payrollops.Tests.Payslip.PayslipAssignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Payslip.PayslipAssignment;

/// <summary>Checks of payslip assignment code, run by the project's runner.</summary>
public static class RollingPayslipAssignmentEngineChecks
{
    public static void RollingPayslipAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = RollingPayslipAssignmentEngine.DefaultRollingPayslipAssignmentEngine(Flags.NewRegistry());
        var item = RollingPayslipAssignment.NewRollingPayslipAssignment("code-222", "label-856");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RollingPayslipAssignmentEngineKeepsResultsWithinBounds();
    }
}
