namespace Acme.Workforce.Compliance.Tests.Attestation.Window;

using Acme.Workforce.Compliance.Attestation.Window;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class AttestationAgreementEngineChecks
{
    public static void AttestationAgreementEngineKeepsResultsWithinBounds()
    {
        var engine = AttestationAgreementEngine.DefaultAttestationAgreementEngine(Flags.NewRegistry());
        var item = AttestationAgreement.NewAttestationAgreement("code-417", "label-6");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AttestationAgreementEngineKeepsResultsWithinBounds();
    }
}
