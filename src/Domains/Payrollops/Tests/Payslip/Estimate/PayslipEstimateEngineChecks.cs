namespace Acme.Workforce.Payrollops.Tests.Payslip.Estimate;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Payslip.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PayslipEstimateEngineChecks
{
    public static void PayslipEstimateEngineKeepsResultsWithinBounds()
    {
        var engine = PayslipEstimateEngine.DefaultPayslipEstimateEngine(Flags.NewRegistry());
        var item = PayslipEstimate.NewPayslipEstimate("code-660", "name-552");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PayslipEstimateEngineKeepsResultsWithinBounds();
    }
}
