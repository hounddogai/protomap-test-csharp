namespace Acme.Workforce.Compliance.Tests.Attestation.RecordItem;

using Acme.Workforce.Compliance.Attestation.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class AdaptiveAttestationRecordEngineChecks
{
    public static void AdaptiveAttestationRecordEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveAttestationRecordEngine.DefaultAdaptiveAttestationRecordEngine();
        var item = AdaptiveAttestationRecord.NewAdaptiveAttestationRecord("code-640", "label-295");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveAttestationRecordEngineKeepsResultsWithinBounds();
    }
}
