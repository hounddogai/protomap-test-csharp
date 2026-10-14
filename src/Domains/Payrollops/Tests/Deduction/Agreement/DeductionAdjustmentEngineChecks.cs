namespace Acme.Workforce.Payrollops.Tests.Deduction.Agreement;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Deduction.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class DeductionAdjustmentEngineChecks
{
    public static void DeductionAdjustmentEngineKeepsResultsWithinBounds()
    {
        var engine = DeductionAdjustmentEngine.DefaultDeductionAdjustmentEngine(Flags.NewRegistry());
        var item = DeductionAdjustment.NewDeductionAdjustment("code-251", "title-745");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DeductionAdjustmentEngineKeepsResultsWithinBounds();
    }
}
