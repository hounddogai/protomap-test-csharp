namespace Acme.Workforce.Compliance.Tests.Attestation.AttestationRule;

using Acme.Workforce.Compliance.Attestation.AttestationRule;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of attestation rule code, run by the project's runner.</summary>
public static class AttestationThresholdEngineChecks
{
    public static void AttestationThresholdEngineKeepsResultsWithinBounds()
    {
        var engine = AttestationThresholdEngine.DefaultAttestationThresholdEngine(Flags.NewRegistry());
        var item = AttestationThreshold.NewAttestationThreshold("code-972", "name-614");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AttestationThresholdEngineKeepsResultsWithinBounds();
    }
}
