namespace Acme.Workforce.Payrollops.Tests.Deduction.Exception;

using Acme.Workforce.Payrollops.Deduction.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class AdaptiveDeductionBaselineEngineChecks
{
    public static void AdaptiveDeductionBaselineEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveDeductionBaselineEngine.DefaultAdaptiveDeductionBaselineEngine();
        var item = AdaptiveDeductionBaseline.NewAdaptiveDeductionBaseline("code-969", "title-124");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveDeductionBaselineEngineKeepsResultsWithinBounds();
    }
}
