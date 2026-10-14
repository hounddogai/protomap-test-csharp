namespace Acme.Workforce.Assets.Tests.Assignment.Claim;

using Acme.Workforce.Assets.Assignment.Claim;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class ScheduledAssignmentCheckpointEngineChecks
{
    public static void ScheduledAssignmentCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledAssignmentCheckpointEngine.DefaultScheduledAssignmentCheckpointEngine(Flags.NewRegistry());
        var item = ScheduledAssignmentCheckpoint.NewScheduledAssignmentCheckpoint("code-3", "title-733");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledAssignmentCheckpointEngineKeepsResultsWithinBounds();
    }
}
