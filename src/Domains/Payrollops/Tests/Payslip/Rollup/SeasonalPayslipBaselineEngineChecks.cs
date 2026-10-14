namespace Acme.Workforce.Payrollops.Tests.Payslip.Rollup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Payslip.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class SeasonalPayslipBaselineEngineChecks
{
    public static void SeasonalPayslipBaselineEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalPayslipBaselineEngine.DefaultSeasonalPayslipBaselineEngine(Flags.NewRegistry());
        var item = SeasonalPayslipBaseline.NewSeasonalPayslipBaseline("code-581", "label-999");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalPayslipBaselineEngineKeepsResultsWithinBounds();
    }
}
