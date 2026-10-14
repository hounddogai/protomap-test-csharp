namespace Acme.Workforce.Compliance.Tests.Attestation.Template;

using Acme.Workforce.Compliance.Attestation.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class LegacyAttestationEntryEngineChecks
{
    public static void LegacyAttestationEntryEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyAttestationEntryEngine.DefaultLegacyAttestationEntryEngine();
        var item = LegacyAttestationEntry.NewLegacyAttestationEntry("code-377", "label-36");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyAttestationEntryEngineKeepsResultsWithinBounds();
    }
}
