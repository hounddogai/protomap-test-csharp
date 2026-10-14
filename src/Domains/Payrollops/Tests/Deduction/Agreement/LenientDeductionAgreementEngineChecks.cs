namespace Acme.Workforce.Payrollops.Tests.Deduction.Agreement;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Deduction.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class LenientDeductionAgreementEngineChecks
{
    public static void LenientDeductionAgreementEngineKeepsResultsWithinBounds()
    {
        var engine = LenientDeductionAgreementEngine.DefaultLenientDeductionAgreementEngine(Flags.NewRegistry());
        var item = LenientDeductionAgreement.NewLenientDeductionAgreement("code-859", "title-29");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientDeductionAgreementEngineKeepsResultsWithinBounds();
    }
}
