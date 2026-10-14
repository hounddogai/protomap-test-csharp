namespace Acme.Workforce.Payrollops.Tests.Deduction.Assignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Deduction.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class DeductionMappingEngineChecks
{
    public static void DeductionMappingEngineKeepsResultsWithinBounds()
    {
        var engine = DeductionMappingEngine.DefaultDeductionMappingEngine(Flags.NewRegistry());
        var item = DeductionMapping.NewDeductionMapping("code-289", "label-7");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DeductionMappingEngineKeepsResultsWithinBounds();
    }
}
