namespace Acme.Workforce.Compliance.Tests.Attestation.RecordItem;

using Acme.Workforce.Compliance.Attestation.RecordItem;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class DefaultAttestationRollupEngineChecks
{
    public static void DefaultAttestationRollupEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultAttestationRollupEngine.DefaultDefaultAttestationRollupEngine(Flags.NewRegistry());
        var item = DefaultAttestationRollup.NewDefaultAttestationRollup("code-914", "label-874");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultAttestationRollupEngineKeepsResultsWithinBounds();
    }
}
