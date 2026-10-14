namespace Acme.Workforce.Compliance.Tests.Attestation.Policy;

using Acme.Workforce.Compliance.Attestation.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class AttestationOverrideEngineChecks
{
    public static void AttestationOverrideEngineKeepsResultsWithinBounds()
    {
        var engine = AttestationOverrideEngine.DefaultAttestationOverrideEngine();
        var item = AttestationOverride.NewAttestationOverride("code-413", "label-847");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AttestationOverrideEngineKeepsResultsWithinBounds();
    }
}
