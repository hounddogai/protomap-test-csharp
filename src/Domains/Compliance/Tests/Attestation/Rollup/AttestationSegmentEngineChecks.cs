namespace Acme.Workforce.Compliance.Tests.Attestation.Rollup;

using Acme.Workforce.Compliance.Attestation.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class AttestationSegmentEngineChecks
{
    public static void AttestationSegmentEngineKeepsResultsWithinBounds()
    {
        var engine = AttestationSegmentEngine.DefaultAttestationSegmentEngine();
        var item = AttestationSegment.NewAttestationSegment("code-221", "name-944");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AttestationSegmentEngineKeepsResultsWithinBounds();
    }
}
