namespace Acme.Workforce.Payrollops.Tests.Deduction.Policy;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Deduction.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class DeductionOverrideEngineChecks
{
    public static void DeductionOverrideEngineKeepsResultsWithinBounds()
    {
        var engine = DeductionOverrideEngine.DefaultDeductionOverrideEngine(Flags.NewRegistry());
        var item = DeductionOverride.NewDeductionOverride("code-570", "name-984");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DeductionOverrideEngineKeepsResultsWithinBounds();
    }
}
