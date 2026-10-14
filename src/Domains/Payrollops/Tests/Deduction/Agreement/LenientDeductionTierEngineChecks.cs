namespace Acme.Workforce.Payrollops.Tests.Deduction.Agreement;

using Acme.Workforce.Payrollops.Deduction.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class LenientDeductionTierEngineChecks
{
    public static void LenientDeductionTierEngineKeepsResultsWithinBounds()
    {
        var engine = LenientDeductionTierEngine.DefaultLenientDeductionTierEngine();
        var item = LenientDeductionTier.NewLenientDeductionTier("code-380", "name-549");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientDeductionTierEngineKeepsResultsWithinBounds();
    }
}
