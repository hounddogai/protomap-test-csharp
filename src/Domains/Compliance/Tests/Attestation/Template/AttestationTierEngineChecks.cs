namespace Acme.Workforce.Compliance.Tests.Attestation.Template;

using Acme.Workforce.Compliance.Attestation.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class AttestationTierEngineChecks
{
    public static void AttestationTierEngineKeepsResultsWithinBounds()
    {
        var engine = AttestationTierEngine.DefaultAttestationTierEngine();
        var item = AttestationTier.NewAttestationTier("code-309", "label-465");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AttestationTierEngineKeepsResultsWithinBounds();
    }
}
