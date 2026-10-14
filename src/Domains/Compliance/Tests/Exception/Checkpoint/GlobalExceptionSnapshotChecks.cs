namespace Acme.Workforce.Compliance.Tests.Exception.Checkpoint;

using Acme.Workforce.Compliance.Exception.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class GlobalExceptionSnapshotChecks
{
    public static void GlobalExceptionSnapshotStartsInCreated()
    {
        var item = GlobalExceptionSnapshot.NewGlobalExceptionSnapshot("code-513", "title-154");
        Check.Equal(item.Status, GlobalExceptionSnapshotStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalExceptionSnapshotMovesToRetired()
    {
        var item = GlobalExceptionSnapshot.NewGlobalExceptionSnapshot("code-513", "title-154");
        try
        {
            item.MoveTo(GlobalExceptionSnapshotStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalExceptionSnapshotStatus.Retired, "status");
    }

    public static void GlobalExceptionSnapshotRejectsMovesFromRetired()
    {
        var item = GlobalExceptionSnapshot.NewGlobalExceptionSnapshot("code-513", "title-154");
        item.Status = GlobalExceptionSnapshotStatus.Retired;
        Check.Fails(() => item.MoveTo(GlobalExceptionSnapshotStatus.Created), "move");
    }

    public static void RunAll()
    {
        GlobalExceptionSnapshotStartsInCreated();
        GlobalExceptionSnapshotMovesToRetired();
        GlobalExceptionSnapshotRejectsMovesFromRetired();
    }
}
