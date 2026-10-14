namespace Acme.Workforce.Compliance.Tests.Attestation.Claim;

using Acme.Workforce.Compliance.Attestation.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class AttestationMilestoneEngineChecks
{
    public static void AttestationMilestoneEngineKeepsResultsWithinBounds()
    {
        var engine = AttestationMilestoneEngine.DefaultAttestationMilestoneEngine();
        var item = AttestationMilestone.NewAttestationMilestone("code-202", "label-844");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AttestationMilestoneEngineKeepsResultsWithinBounds();
    }
}
