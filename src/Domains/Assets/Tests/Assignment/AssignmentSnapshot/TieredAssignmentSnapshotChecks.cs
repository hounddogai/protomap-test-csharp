namespace Acme.Workforce.Assets.Tests.Assignment.AssignmentSnapshot;

using Acme.Workforce.Assets.Assignment.AssignmentSnapshot;

/// <summary>Checks of assignment snapshot code, run by the project's runner.</summary>
public static class TieredAssignmentSnapshotChecks
{
    public static void TieredAssignmentSnapshotStartsInIdle()
    {
        var item = TieredAssignmentSnapshot.NewTieredAssignmentSnapshot("code-622", "name-451");
        Check.Equal(item.Status, TieredAssignmentSnapshotStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredAssignmentSnapshotMovesToDraining()
    {
        var item = TieredAssignmentSnapshot.NewTieredAssignmentSnapshot("code-622", "name-451");
        try
        {
            item.MoveTo(TieredAssignmentSnapshotStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredAssignmentSnapshotStatus.Draining, "status");
    }

    public static void TieredAssignmentSnapshotRejectsMovesFromStopped()
    {
        var item = TieredAssignmentSnapshot.NewTieredAssignmentSnapshot("code-622", "name-451");
        item.Status = TieredAssignmentSnapshotStatus.Stopped;
        Check.Fails(() => item.MoveTo(TieredAssignmentSnapshotStatus.Idle), "move");
    }

    public static void RunAll()
    {
        TieredAssignmentSnapshotStartsInIdle();
        TieredAssignmentSnapshotMovesToDraining();
        TieredAssignmentSnapshotRejectsMovesFromStopped();
    }
}
