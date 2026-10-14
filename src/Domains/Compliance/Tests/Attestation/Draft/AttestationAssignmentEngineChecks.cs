namespace Acme.Workforce.Compliance.Tests.Attestation.Draft;

using Acme.Workforce.Compliance.Attestation.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class AttestationAssignmentEngineChecks
{
    public static void AttestationAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = AttestationAssignmentEngine.DefaultAttestationAssignmentEngine();
        var item = AttestationAssignment.NewAttestationAssignment("code-936", "label-237");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AttestationAssignmentEngineKeepsResultsWithinBounds();
    }
}
