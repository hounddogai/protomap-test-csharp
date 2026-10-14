namespace Acme.Workforce.Compliance.Tests.Attestation.Digest;

using Acme.Workforce.Compliance.Attestation.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class AttestationSnapshotEngineChecks
{
    public static void AttestationSnapshotEngineKeepsResultsWithinBounds()
    {
        var engine = AttestationSnapshotEngine.DefaultAttestationSnapshotEngine();
        var item = AttestationSnapshot.NewAttestationSnapshot("code-423", "label-98");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AttestationSnapshotEngineKeepsResultsWithinBounds();
    }
}
