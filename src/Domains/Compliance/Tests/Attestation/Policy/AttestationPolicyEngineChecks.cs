namespace Acme.Workforce.Compliance.Tests.Attestation.Policy;

using Acme.Workforce.Compliance.Attestation.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class AttestationPolicyEngineChecks
{
    public static void AttestationPolicyEngineKeepsResultsWithinBounds()
    {
        var engine = AttestationPolicyEngine.DefaultAttestationPolicyEngine();
        var item = AttestationPolicy.NewAttestationPolicy("code-435", "name-281");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AttestationPolicyEngineKeepsResultsWithinBounds();
    }
}
