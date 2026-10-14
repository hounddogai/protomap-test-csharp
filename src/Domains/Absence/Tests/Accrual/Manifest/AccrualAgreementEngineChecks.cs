namespace Acme.Workforce.Absence.Tests.Accrual.Manifest;

using Acme.Workforce.Absence.Accrual.Manifest;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class AccrualAgreementEngineChecks
{
    public static void AccrualAgreementEngineKeepsResultsWithinBounds()
    {
        var engine = AccrualAgreementEngine.DefaultAccrualAgreementEngine(Flags.NewRegistry());
        var item = AccrualAgreement.NewAccrualAgreement("code-969", "title-95");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AccrualAgreementEngineKeepsResultsWithinBounds();
    }
}
