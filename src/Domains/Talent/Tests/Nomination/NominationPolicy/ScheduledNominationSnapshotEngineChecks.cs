namespace Acme.Workforce.Talent.Tests.Nomination.NominationPolicy;

using Acme.Workforce.Talent.Nomination.NominationPolicy;

/// <summary>Checks of nomination policy code, run by the project's runner.</summary>
public static class ScheduledNominationSnapshotEngineChecks
{
    public static void ScheduledNominationSnapshotEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledNominationSnapshotEngine.DefaultScheduledNominationSnapshotEngine();
        var item = ScheduledNominationSnapshot.NewScheduledNominationSnapshot("code-10", "title-707");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledNominationSnapshotEngineKeepsResultsWithinBounds();
    }
}
