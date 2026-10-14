namespace Acme.Workforce.Payrollops.Tests.Deduction.Slot;

using Acme.Workforce.Payrollops.Deduction.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class DeductionAgreementEngineChecks
{
    public static void DeductionAgreementEngineKeepsResultsWithinBounds()
    {
        var engine = DeductionAgreementEngine.DefaultDeductionAgreementEngine();
        var item = DeductionAgreement.NewDeductionAgreement("code-145", "label-982");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DeductionAgreementEngineKeepsResultsWithinBounds();
    }
}
