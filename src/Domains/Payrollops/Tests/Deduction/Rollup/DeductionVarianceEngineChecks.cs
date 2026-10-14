namespace Acme.Workforce.Payrollops.Tests.Deduction.Rollup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Deduction.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class DeductionVarianceEngineChecks
{
    public static void DeductionVarianceEngineKeepsResultsWithinBounds()
    {
        var engine = DeductionVarianceEngine.DefaultDeductionVarianceEngine(Flags.NewRegistry());
        var item = DeductionVariance.NewDeductionVariance("code-902", "title-344");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DeductionVarianceEngineKeepsResultsWithinBounds();
    }
}
