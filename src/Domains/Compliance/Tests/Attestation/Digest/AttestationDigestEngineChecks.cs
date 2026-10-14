namespace Acme.Workforce.Compliance.Tests.Attestation.Digest;

using Acme.Workforce.Compliance.Attestation.Digest;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class AttestationDigestEngineChecks
{
    public static void AttestationDigestEngineKeepsResultsWithinBounds()
    {
        var engine = AttestationDigestEngine.DefaultAttestationDigestEngine(Flags.NewRegistry());
        var item = AttestationDigest.NewAttestationDigest("code-429", "title-860");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AttestationDigestEngineKeepsResultsWithinBounds();
    }
}
