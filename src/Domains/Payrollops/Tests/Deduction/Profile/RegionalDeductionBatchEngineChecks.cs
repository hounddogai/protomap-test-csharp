namespace Acme.Workforce.Payrollops.Tests.Deduction.Profile;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Deduction.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class RegionalDeductionBatchEngineChecks
{
    public static void RegionalDeductionBatchEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalDeductionBatchEngine.DefaultRegionalDeductionBatchEngine(Flags.NewRegistry());
        var item = RegionalDeductionBatch.NewRegionalDeductionBatch("code-563", "name-304");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalDeductionBatchEngineKeepsResultsWithinBounds();
    }
}
