namespace Acme.Workforce.Compliance.Tests.Attestation.Schedule;

using Acme.Workforce.Compliance.Attestation.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class DefaultAttestationClaimEngineChecks
{
    public static void DefaultAttestationClaimEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultAttestationClaimEngine.DefaultDefaultAttestationClaimEngine();
        var item = DefaultAttestationClaim.NewDefaultAttestationClaim("code-884", "name-957");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultAttestationClaimEngineKeepsResultsWithinBounds();
    }
}
