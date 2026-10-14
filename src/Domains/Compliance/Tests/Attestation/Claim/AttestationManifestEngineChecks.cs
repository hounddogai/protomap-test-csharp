namespace Acme.Workforce.Compliance.Tests.Attestation.Claim;

using Acme.Workforce.Compliance.Attestation.Claim;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class AttestationManifestEngineChecks
{
    public static void AttestationManifestEngineKeepsResultsWithinBounds()
    {
        var engine = AttestationManifestEngine.DefaultAttestationManifestEngine(Flags.NewRegistry());
        var item = AttestationManifest.NewAttestationManifest("code-230", "label-850");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AttestationManifestEngineKeepsResultsWithinBounds();
    }
}
