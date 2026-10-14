namespace Acme.Workforce.Compliance.Tests.Attestation.AttestationEstimate;

using Acme.Workforce.Compliance.Attestation.AttestationEstimate;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of attestation estimate code, run by the project's runner.</summary>
public static class ManualAttestationEstimateEngineChecks
{
    public static void ManualAttestationEstimateEngineKeepsResultsWithinBounds()
    {
        var engine = ManualAttestationEstimateEngine.DefaultManualAttestationEstimateEngine(Flags.NewRegistry());
        var item = ManualAttestationEstimate.NewManualAttestationEstimate("code-142", "label-981");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ManualAttestationEstimateEngineKeepsResultsWithinBounds();
    }
}
