namespace Acme.Workforce.Talent.Tests.Pool.Target;

using Acme.Workforce.Talent.Pool.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class PoolSnapshotChecks
{
    public static void PoolSnapshotStartsInIdle()
    {
        var item = PoolSnapshot.NewPoolSnapshot("code-780", "title-241");
        Check.Equal(item.Status, PoolSnapshotStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PoolSnapshotMovesToWarming()
    {
        var item = PoolSnapshot.NewPoolSnapshot("code-780", "title-241");
        try
        {
            item.MoveTo(PoolSnapshotStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PoolSnapshotStatus.Warming, "status");
    }

    public static void PoolSnapshotRejectsMovesFromStopped()
    {
        var item = PoolSnapshot.NewPoolSnapshot("code-780", "title-241");
        item.Status = PoolSnapshotStatus.Stopped;
        Check.Fails(() => item.MoveTo(PoolSnapshotStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PoolSnapshotStartsInIdle();
        PoolSnapshotMovesToWarming();
        PoolSnapshotRejectsMovesFromStopped();
    }
}
