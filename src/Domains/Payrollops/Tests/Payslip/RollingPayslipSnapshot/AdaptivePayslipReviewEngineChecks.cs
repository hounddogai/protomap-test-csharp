namespace Acme.Workforce.Payrollops.Tests.Payslip.RollingPayslipSnapshot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Payslip.RollingPayslipSnapshot;

/// <summary>Checks of rolling payslip snapshot code, run by the project's runner.</summary>
public static class AdaptivePayslipReviewEngineChecks
{
    public static void AdaptivePayslipReviewEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptivePayslipReviewEngine.DefaultAdaptivePayslipReviewEngine(Flags.NewRegistry());
        var item = AdaptivePayslipReview.NewAdaptivePayslipReview("code-751", "name-822");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptivePayslipReviewEngineKeepsResultsWithinBounds();
    }
}
