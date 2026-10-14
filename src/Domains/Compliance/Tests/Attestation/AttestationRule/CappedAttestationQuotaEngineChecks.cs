namespace Acme.Workforce.Compliance.Tests.Attestation.AttestationRule;

using Acme.Workforce.Compliance.Attestation.AttestationRule;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of attestation rule code, run by the project's runner.</summary>
public static class CappedAttestationQuotaEngineChecks
{
    public static void CappedAttestationQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = CappedAttestationQuotaEngine.DefaultCappedAttestationQuotaEngine(Flags.NewRegistry());
        var item = CappedAttestationQuota.NewCappedAttestationQuota("code-760", "label-986");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedAttestationQuotaEngineKeepsResultsWithinBounds();
    }
}
