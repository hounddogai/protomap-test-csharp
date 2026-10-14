namespace Acme.Workforce.Compliance.Tests.Attestation.AttestationEstimate;

using Acme.Workforce.Compliance.Attestation.AttestationEstimate;

/// <summary>Checks of attestation estimate code, run by the project's runner.</summary>
public static class StrictAttestationEstimateEngineChecks
{
    public static void StrictAttestationEstimateEngineKeepsResultsWithinBounds()
    {
        var engine = StrictAttestationEstimateEngine.DefaultStrictAttestationEstimateEngine();
        var item = StrictAttestationEstimate.NewStrictAttestationEstimate("code-219", "name-582");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictAttestationEstimateEngineKeepsResultsWithinBounds();
    }
}
