namespace Acme.Workforce.Compliance.Tests.Attestation.Checkpoint;

using Acme.Workforce.Compliance.Attestation.Checkpoint;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class GlobalAttestationCheckpointEngineChecks
{
    public static void GlobalAttestationCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalAttestationCheckpointEngine.DefaultGlobalAttestationCheckpointEngine(Flags.NewRegistry());
        var item = GlobalAttestationCheckpoint.NewGlobalAttestationCheckpoint("code-291", "label-713");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalAttestationCheckpointEngineKeepsResultsWithinBounds();
    }
}
