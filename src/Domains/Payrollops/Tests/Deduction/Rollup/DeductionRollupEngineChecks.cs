namespace Acme.Workforce.Payrollops.Tests.Deduction.Rollup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Deduction.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class DeductionRollupEngineChecks
{
    public static void DeductionRollupEngineKeepsResultsWithinBounds()
    {
        var engine = DeductionRollupEngine.DefaultDeductionRollupEngine(Flags.NewRegistry());
        var item = DeductionRollup.NewDeductionRollup("code-190", "name-688");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DeductionRollupEngineKeepsResultsWithinBounds();
    }
}
