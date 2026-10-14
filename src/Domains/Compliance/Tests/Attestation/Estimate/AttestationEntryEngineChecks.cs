namespace Acme.Workforce.Compliance.Tests.Attestation.Estimate;

using Acme.Workforce.Compliance.Attestation.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class AttestationEntryEngineChecks
{
    public static void AttestationEntryEngineKeepsResultsWithinBounds()
    {
        var engine = AttestationEntryEngine.DefaultAttestationEntryEngine();
        var item = AttestationEntry.NewAttestationEntry("code-882", "label-308");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AttestationEntryEngineKeepsResultsWithinBounds();
    }
}
