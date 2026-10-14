namespace Acme.Workforce.Talent.Tests.Nomination.Checkpoint;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Nomination.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class NominationSnapshotEngineChecks
{
    public static void NominationSnapshotEngineKeepsResultsWithinBounds()
    {
        var engine = NominationSnapshotEngine.DefaultNominationSnapshotEngine(Flags.NewRegistry());
        var item = NominationSnapshot.NewNominationSnapshot("code-598", "name-450");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        NominationSnapshotEngineKeepsResultsWithinBounds();
    }
}
