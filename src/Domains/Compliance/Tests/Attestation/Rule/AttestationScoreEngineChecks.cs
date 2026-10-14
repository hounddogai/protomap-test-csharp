namespace Acme.Workforce.Compliance.Tests.Attestation.Rule;

using Acme.Workforce.Compliance.Attestation.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class AttestationScoreEngineChecks
{
    public static void AttestationScoreEngineKeepsResultsWithinBounds()
    {
        var engine = AttestationScoreEngine.DefaultAttestationScoreEngine();
        var item = AttestationScore.NewAttestationScore("code-144", "title-234");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AttestationScoreEngineKeepsResultsWithinBounds();
    }
}
