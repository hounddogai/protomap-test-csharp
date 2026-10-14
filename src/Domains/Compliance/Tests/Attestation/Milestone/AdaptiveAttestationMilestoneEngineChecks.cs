namespace Acme.Workforce.Compliance.Tests.Attestation.Milestone;

using Acme.Workforce.Compliance.Attestation.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class AdaptiveAttestationMilestoneEngineChecks
{
    public static void AdaptiveAttestationMilestoneEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveAttestationMilestoneEngine.DefaultAdaptiveAttestationMilestoneEngine();
        var item = AdaptiveAttestationMilestone.NewAdaptiveAttestationMilestone("code-332", "name-331");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveAttestationMilestoneEngineKeepsResultsWithinBounds();
    }
}
