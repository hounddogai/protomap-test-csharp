namespace Acme.Workforce.Compliance.Tests.Attestation.Window;

using Acme.Workforce.Compliance.Attestation.Window;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class AttestationCheckpointEngineChecks
{
    public static void AttestationCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = AttestationCheckpointEngine.DefaultAttestationCheckpointEngine(Flags.NewRegistry());
        var item = AttestationCheckpoint.NewAttestationCheckpoint("code-122", "label-697");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AttestationCheckpointEngineKeepsResultsWithinBounds();
    }
}
