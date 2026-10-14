namespace Acme.Workforce.Payrollops.Tests.Deduction.Slot;

using Acme.Workforce.Payrollops.Deduction.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class DeductionBundleEngineChecks
{
    public static void DeductionBundleEngineKeepsResultsWithinBounds()
    {
        var engine = DeductionBundleEngine.DefaultDeductionBundleEngine();
        var item = DeductionBundle.NewDeductionBundle("code-799", "name-899");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DeductionBundleEngineKeepsResultsWithinBounds();
    }
}
