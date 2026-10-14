namespace Acme.Workforce.Compliance.Tests.Attestation.Checkpoint;

using Acme.Workforce.Compliance.Attestation.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class AttestationRequestEngineChecks
{
    public static void AttestationRequestEngineKeepsResultsWithinBounds()
    {
        var engine = AttestationRequestEngine.DefaultAttestationRequestEngine();
        var item = AttestationRequest.NewAttestationRequest("code-870", "label-709");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AttestationRequestEngineKeepsResultsWithinBounds();
    }
}
