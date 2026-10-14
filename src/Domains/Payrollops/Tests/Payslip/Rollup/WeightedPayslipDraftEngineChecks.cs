namespace Acme.Workforce.Payrollops.Tests.Payslip.Rollup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Payslip.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class WeightedPayslipDraftEngineChecks
{
    public static void WeightedPayslipDraftEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedPayslipDraftEngine.DefaultWeightedPayslipDraftEngine(Flags.NewRegistry());
        var item = WeightedPayslipDraft.NewWeightedPayslipDraft("code-738", "label-979");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedPayslipDraftEngineKeepsResultsWithinBounds();
    }
}
