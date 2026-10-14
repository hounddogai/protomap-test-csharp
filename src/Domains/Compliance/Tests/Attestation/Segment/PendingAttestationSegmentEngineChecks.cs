namespace Acme.Workforce.Compliance.Tests.Attestation.Segment;

using Acme.Workforce.Compliance.Attestation.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class PendingAttestationSegmentEngineChecks
{
    public static void PendingAttestationSegmentEngineKeepsResultsWithinBounds()
    {
        var engine = PendingAttestationSegmentEngine.DefaultPendingAttestationSegmentEngine();
        var item = PendingAttestationSegment.NewPendingAttestationSegment("code-294", "title-302");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingAttestationSegmentEngineKeepsResultsWithinBounds();
    }
}
