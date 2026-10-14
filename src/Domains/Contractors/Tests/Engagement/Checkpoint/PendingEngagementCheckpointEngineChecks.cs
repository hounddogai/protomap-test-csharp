namespace Acme.Workforce.Contractors.Tests.Engagement.Checkpoint;

using Acme.Workforce.Contractors.Engagement.Checkpoint;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class PendingEngagementCheckpointEngineChecks
{
    public static void PendingEngagementCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = PendingEngagementCheckpointEngine.DefaultPendingEngagementCheckpointEngine(Flags.NewRegistry());
        var item = PendingEngagementCheckpoint.NewPendingEngagementCheckpoint("code-302", "title-127");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingEngagementCheckpointEngineKeepsResultsWithinBounds();
    }
}
