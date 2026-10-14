namespace Acme.Workforce.Contractors.Tests.Engagement.EngagementSnapshot;

using Acme.Workforce.Contractors.Engagement.EngagementSnapshot;

/// <summary>Checks of engagement snapshot code, run by the project's runner.</summary>
public static class StrictEngagementSnapshotChecks
{
    public static void StrictEngagementSnapshotStartsInCreated()
    {
        var item = StrictEngagementSnapshot.NewStrictEngagementSnapshot("code-824", "title-67");
        Check.Equal(item.Status, StrictEngagementSnapshotStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictEngagementSnapshotMovesToPublished()
    {
        var item = StrictEngagementSnapshot.NewStrictEngagementSnapshot("code-824", "title-67");
        try
        {
            item.MoveTo(StrictEngagementSnapshotStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictEngagementSnapshotStatus.Published, "status");
    }

    public static void StrictEngagementSnapshotRejectsMovesFromRetired()
    {
        var item = StrictEngagementSnapshot.NewStrictEngagementSnapshot("code-824", "title-67");
        item.Status = StrictEngagementSnapshotStatus.Retired;
        Check.Fails(() => item.MoveTo(StrictEngagementSnapshotStatus.Created), "move");
    }

    public static void RunAll()
    {
        StrictEngagementSnapshotStartsInCreated();
        StrictEngagementSnapshotMovesToPublished();
        StrictEngagementSnapshotRejectsMovesFromRetired();
    }
}
