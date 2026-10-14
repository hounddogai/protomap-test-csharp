namespace Acme.Workforce.Payrollops.Tests.Deduction.DeductionBaseline;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Deduction.DeductionBaseline;

/// <summary>Checks of deduction baseline code, run by the project's runner.</summary>
public static class GlobalDeductionBaselineEngineChecks
{
    public static void GlobalDeductionBaselineEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalDeductionBaselineEngine.DefaultGlobalDeductionBaselineEngine(Flags.NewRegistry());
        var item = GlobalDeductionBaseline.NewGlobalDeductionBaseline("code-976", "label-681");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalDeductionBaselineEngineKeepsResultsWithinBounds();
    }
}
