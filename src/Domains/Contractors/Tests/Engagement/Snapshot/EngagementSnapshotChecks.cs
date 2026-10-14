namespace Acme.Workforce.Contractors.Tests.Engagement.Snapshot;

using Acme.Workforce.Contractors.Engagement.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class EngagementSnapshotChecks
{
    public static void EngagementSnapshotStartsInIdle()
    {
        var item = EngagementSnapshot.NewEngagementSnapshot("code-385", "title-461");
        Check.Equal(item.Status, EngagementSnapshotStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EngagementSnapshotMovesToReady()
    {
        var item = EngagementSnapshot.NewEngagementSnapshot("code-385", "title-461");
        try
        {
            item.MoveTo(EngagementSnapshotStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EngagementSnapshotStatus.Ready, "status");
    }

    public static void EngagementSnapshotRejectsMovesFromStopped()
    {
        var item = EngagementSnapshot.NewEngagementSnapshot("code-385", "title-461");
        item.Status = EngagementSnapshotStatus.Stopped;
        Check.Fails(() => item.MoveTo(EngagementSnapshotStatus.Idle), "move");
    }

    public static void RunAll()
    {
        EngagementSnapshotStartsInIdle();
        EngagementSnapshotMovesToReady();
        EngagementSnapshotRejectsMovesFromStopped();
    }
}
