namespace Acme.Workforce.Payrollops.Tests.Payslip.Batch;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Payslip.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class PayslipAdjustmentEngineChecks
{
    public static void PayslipAdjustmentEngineKeepsResultsWithinBounds()
    {
        var engine = PayslipAdjustmentEngine.DefaultPayslipAdjustmentEngine(Flags.NewRegistry());
        var item = PayslipAdjustment.NewPayslipAdjustment("code-920", "name-767");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PayslipAdjustmentEngineKeepsResultsWithinBounds();
    }
}
