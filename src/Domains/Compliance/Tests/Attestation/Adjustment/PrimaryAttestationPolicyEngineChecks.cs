namespace Acme.Workforce.Compliance.Tests.Attestation.Adjustment;

using Acme.Workforce.Compliance.Attestation.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class PrimaryAttestationPolicyEngineChecks
{
    public static void PrimaryAttestationPolicyEngineKeepsResultsWithinBounds()
    {
        var engine = PrimaryAttestationPolicyEngine.DefaultPrimaryAttestationPolicyEngine();
        var item = PrimaryAttestationPolicy.NewPrimaryAttestationPolicy("code-959", "name-898");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PrimaryAttestationPolicyEngineKeepsResultsWithinBounds();
    }
}
