namespace Acme.Workforce.Talent.Tests.Nomination.Checkpoint;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Nomination.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class NominationCheckpointEngineChecks
{
    public static void NominationCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = NominationCheckpointEngine.DefaultNominationCheckpointEngine(Flags.NewRegistry());
        var item = NominationCheckpoint.NewNominationCheckpoint("code-198", "title-553");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        NominationCheckpointEngineKeepsResultsWithinBounds();
    }
}
