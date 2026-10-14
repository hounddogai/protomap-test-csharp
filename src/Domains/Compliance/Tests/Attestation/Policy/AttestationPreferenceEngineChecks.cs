namespace Acme.Workforce.Compliance.Tests.Attestation.Policy;

using Acme.Workforce.Compliance.Attestation.Policy;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class AttestationPreferenceEngineChecks
{
    public static void AttestationPreferenceEngineKeepsResultsWithinBounds()
    {
        var engine = AttestationPreferenceEngine.DefaultAttestationPreferenceEngine(Flags.NewRegistry());
        var item = AttestationPreference.NewAttestationPreference("code-25", "label-548");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AttestationPreferenceEngineKeepsResultsWithinBounds();
    }
}
